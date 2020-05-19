using System;

namespace SmartACDeviceAPI.Exceptions
{

    public class AuthZException : Exception {

        public AuthZException() {
            
        }

        public override string ToString() {
            return "Unauthoized";
        }

    }
    
}