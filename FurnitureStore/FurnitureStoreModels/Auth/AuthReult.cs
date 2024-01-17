using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureStoreModels.Auth
{
    public class AuthReult
    {
        public  string Token { get; set; }
        public bool Result { get; set; }
        public List<string> Errors { get; set; }
    }
}
