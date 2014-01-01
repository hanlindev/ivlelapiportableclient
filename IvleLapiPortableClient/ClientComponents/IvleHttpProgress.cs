using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IvleLapiPortableClient.ClientComponents
{
    public delegate void IvleHttpProgressHandler(IvleHttpProgress progress);

    class IvleHttpProgress
    {
        private long? mBytesReceived;
        private long? mTotalBytesToReceive;
        private long? mBytesSent;
        private long? mTotalBytesToSend;

        public event IvleHttpProgressHandler ProgressUpdatedEvent;

        public long? BytesReceived
        {
            set
            {
                resetSendProgress();
                mBytesReceived = value;
            }
            get
            {
                return mBytesReceived;
            }
        }

        public long? TotalBytesToReceive
        {
            set
            {
                resetSendProgress();
                mTotalBytesToReceive = value;
            }
            get
            {
                return mTotalBytesToReceive;
            }
        }

        public long? BytesSent
        {
            set
            {
                resetReceiveProgress();
                mBytesSent = value;
            }
            get
            {
                return mBytesSent;
            }
        }

        public long? TotalBytesToSend
        {
            set
            {
                resetReceiveProgress();
                mTotalBytesToSend = value;
            }
            get
            {
                return mTotalBytesToSend;
            }
        }

        public bool IsOperationInProgress
        {
            get;
            set;
        }

        public IvleHttpProgress()
        {
            resetSendProgress();
            resetReceiveProgress();
            IsOperationInProgress = false;
        }

        public IvleHttpProgress(IvleHttpProgressHandler defaultHandler): this()
        {
            this.ProgressUpdatedEvent += defaultHandler;
        }

        private void resetSendProgress()
        {
            this.mBytesSent = 0;
            this.mTotalBytesToSend = 0;
        }

        private void resetReceiveProgress()
        {
            this.mBytesReceived = 0;
            this.mTotalBytesToReceive = 0;
        }

        public void report()
        {
            if (this.ProgressUpdatedEvent != null)
            {
                this.ProgressUpdatedEvent(this);
            }
        }
    }
}
