using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.ApStorage
{
    abstract class ApDbInterface
    {
        abstract public bool setVar(string pname, string value);
        abstract public string getVar(string name, string defaultValue);
        abstract public bool setLongVar(byte[] data, string name);
        abstract public byte[] getLongVar(out bool result, string name);

        public byte[] GetBytes(string str, bool completeWidthZero, int minVetSize, bool asAnsiString = true)
        {
            byte[] bytes;
            if (asAnsiString)
            {
                bytes = new byte[str.Length];
                for (int i = 0; i < str.Length; i++)
                    bytes[i] = Convert.ToByte(str[i]);
            }
            else
            {
                bytes = new byte[str.Length * sizeof(char)];
                System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            }

            if ((completeWidthZero == true) && (bytes.Length < minVetSize))
            {
                int cont;

                byte[] ret = new byte[minVetSize];
                for (cont = 0; cont < bytes.Length; cont++)
                    ret[cont] = bytes[cont];

                for (cont = cont; cont < minVetSize; cont++)
                    ret[cont] = 0;

                return ret;

            }
            return bytes;
        }

        public string GetString(byte[] bytes, bool asAsciiBytes = true)
        {

            string retorno = "";

            if (asAsciiBytes)
            {
                for (int cont = 0; cont < bytes.Length && bytes[cont] != 0; cont++)
                    retorno += System.Convert.ToChar(bytes[cont]);

            }
            else
            {
                char[] chars = new char[bytes.Length / sizeof(char)];
                System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
                retorno = new string(chars);
            }
            return retorno;

        }
    }
}
