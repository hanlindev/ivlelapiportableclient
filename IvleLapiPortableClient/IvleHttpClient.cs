// This is the CS4014 not await async call warning
// When an async call is not awaited, it is indeed not needed
// to await it.
#pragma warning disable 4014

using IvleLapiPortableClient.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using IvleLapiPortableClient.ClientComponents;
using System.Threading;
using System.IO;

namespace IvleLapiWinRtClient
{
    /**
     * <summary>
     * <para>The basic generic http client that handles communication with IVLE server
     * and returns data as JsonObjects.
     * <seealso cref="HttpClient"/>
     * </summary>
     */
    public class IvleHttpClient : HttpClient, INotifyPropertyChanged
    {
        private static String BASE_URL = "https://ivle.nus.edu.sg/api/Lapi.svc";
        private static String OPERATION_PROGRESS_PROPERTY = "OperationProgress";
        private static String OPERATION_IN_PROGRESS_PROPERTY = "IsOperationInProgress";

        private static int BUFFER_UNIT_SIZE = 1024;

        private int mOperationProgress;
        private bool mIsOperationInProgress;

        /**
         * <summary>
         * <para>This is the progress of the download or upload operation as percentage represented by
         * an integer from 0 to 100
         * </summary>
         */
        public int OperationProgress
        {
            private set
            {
                if (value > mOperationProgress)
                {
                    mOperationProgress = value;
                    notifyPropertyChange(OPERATION_PROGRESS_PROPERTY);
                }
            }
            get
            {
                return mOperationProgress;
            }
        }

        public bool IsOperationInProgress
        {
            private set
            {
                if (value != mIsOperationInProgress)
                {
                    mIsOperationInProgress = value;
                    notifyPropertyChange(OPERATION_IN_PROGRESS_PROPERTY);
                }
            }
            get
            {
                return mIsOperationInProgress;
            }
        }

        /**
         * <summary>
         * <para>All methods are exception free because not all situations require the very detailed exception
         * information. All the methods will return null if something wrong happened during the operation.
         * If the caller really needs the details about the exception, it can refer to this property.
         * </summary>
         */
        public Exception OperationException
        {
            private set;
            get;
        }

        public IvleCredential Credential
        {
            get;
            set;
        }

        public CancellationToken? OperationCancellationToken
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /**
         * <summary>
         * <para>Constructs an IvleHttpClient using the supplied Lapi API key.
         * <param name="credential">This is a compulsory parameter. It is used to add the 
         * authorization parameters to the request url.</param>
         * </summary>
         * <seealso cref="HttpClient"/>
         */
        public IvleHttpClient(IvleCredential credential) : base()
        {
            this.Credential = credential;
            this.OperationCancellationToken = null;
        }

        public IvleHttpClient(IvleCredential credential, CancellationToken token) : this(credential)
        {
            this.OperationCancellationToken = token;
        }

        private void notifyPropertyChange(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void progressHandler(IvleHttpProgress progress)
        {
            this.IsOperationInProgress = progress.IsOperationInProgress;
            // The same client instance can't download and upload at the same time
            if (progress.BytesReceived != null && progress.TotalBytesToReceive != null)
            {
                this.OperationProgress = (int)(progress.BytesReceived * 100 / progress.TotalBytesToReceive);
            }
            else if (progress.BytesSent != null && progress.TotalBytesToSend != null)
            {
                this.OperationProgress = (int)(progress.BytesSent * 100 / progress.TotalBytesToSend);
            }
        }

        /**
         * <summary>
         * <para>Send the GET request
         * <param name="parameters">The query path including query parameters except authentication parameters.
         * Authentication parameters include APIToken and Token or AuthToken.</param>
         * <returns>The response body as a String or null if the request fails. If the operation failed, the
         * error code will be store
         * </summary>
         */
        public async Task<String> GetStringAsync(String queryPath)
        {
            return await GetStringAsync(BASE_URL, queryPath);
        }

        /**
         * <summary>
         * <para>Send the GET request
         * <param name="baseUrl">The custom base url if https://ivle.nus.edu.sg/api/Lapi.svc is not the desired one.</param>
         * <param name="parameters">The query path including query parameters except authentication parameters.
         * Authentication parameters include APIToken and Token or AuthToken.</param>
         * <returns>The response body as a String or null if the request fails. If the operation failed, the
         * error code will be store
         * </summary>
         */
        public async Task<String> GetStringAsync(String baseUrl, String queryPath)
        {
            String result = null;
            String completePath = baseUrl + queryPath;
            Uri queryUri = new Uri(completePath);

            try
            {
                HttpResponseMessage response = await base.GetAsync(queryUri, HttpCompletionOption.ResponseHeadersRead);
                result = await processResponseAsString(response);
            }
            catch (AggregateException e)
            {
                OperationException = e.InnerException;
            }
            catch (Exception e)
            {
                OperationException = e;
            }
            return result;
        }



        private async Task<String> processResponseAsString(HttpResponseMessage response)
        {
            MemoryStream responseStream = await processResponseAsStream(response);
            // If the response content from IVLE is treated as text, it should only be
            // the case when the request is asking for non-file data. Therefore it is
            // safe to assume that the response is in UTF8 encoding
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            string result = await reader.ReadToEndAsync();
            return result;
        }

        private async Task<T> processResponseAsModel<T>(HttpResponseMessage response)
            where T : ILapiModel, new()
        {
            T result = new T();
            String responseString = await processResponseAsString(response);
            result.Build(responseString);
            return result;
        }

        private async Task<MemoryStream> processResponseAsStream(HttpResponseMessage response)
        {
            byte[] buffer = new byte[BUFFER_UNIT_SIZE];
            IvleHttpProgress progress = new IvleHttpProgress(this.progressHandler);
            HttpContent responseContent = response.Content;
            MemoryStream responseStream = new MemoryStream();
            MemoryStream result = new MemoryStream();
            Task<int> readUnitBytesTask;
            int bytesJustRead = 0;

            progress.TotalBytesToReceive = responseContent.Headers.ContentLength;
            progress.BytesReceived = 0;
            progress.IsOperationInProgress = true;
            progress.report();
            responseContent.CopyToAsync(result);

            do
            {
                if (this.OperationCancellationToken != null)
                {
                    readUnitBytesTask = responseStream.ReadAsync(buffer, 0, BUFFER_UNIT_SIZE, (CancellationToken) this.OperationCancellationToken);
                }
                else
                {
                    readUnitBytesTask = responseStream.ReadAsync(buffer, 0, BUFFER_UNIT_SIZE);
                }
                bytesJustRead = await readUnitBytesTask;
                progress.BytesReceived += bytesJustRead;
                progress.report();
                await result.WriteAsync(buffer, 0, bytesJustRead);
            } while (bytesJustRead == BUFFER_UNIT_SIZE && readUnitBytesTask.Status != TaskStatus.Canceled);

            progress.IsOperationInProgress = false;
            progress.report();

            return result;
        }
    }
}
