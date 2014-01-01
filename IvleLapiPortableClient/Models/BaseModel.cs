using IvleLapiPortableClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IvleLapiPortableClient.Models
{
    public interface ILapiModel
    {
        public ILapiModel(String jsonString)
        {
        }

        public void Build(String jsonString)
        {
        }
    }
}
