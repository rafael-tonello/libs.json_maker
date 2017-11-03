using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Common.ApStorage
{
    public class ApStorage
    {
        int dSize = 50;
        int kSize = 128;
        ApDbInterface confs;

        public ApStorage(string opFilename = "vars.apdb", bool acessoExclusivo = false)
        {
            confs = new ApFileDB2(kSize, dSize, acessoExclusivo, opFilename, false);
        }
       
        bool usando = false;
        public void setConf(string name, string value)
        {
            while (usando)
                System.Threading.Thread.Sleep(1);

            usando = true;
            if (name.Length >= kSize)
                name = name.Substring(0, kSize);
            //verifica se o valor é maior que o tamanho máximo de bytes das chaves (128), se for, utiliza setLongVar
            if (value.Length <= dSize)
                confs.setVar(name, value);
            else
            {
                //invalida a chave padrão
                confs.setVar(name, "----not found----");

                confs.setLongVar(confs.GetBytes(value, false, 0), name);
            }
            usando = false;

        }

        public string getConf(string name, string defValue)
        {
            while (usando)
                System.Threading.Thread.Sleep(1);

            usando = true;

            if (name.Length >= kSize)
                name = name.Substring(0, kSize);
            string retorno = "";
            bool sucess = false;
            //tenta localiza com getVar, caso não encontre, utilza getLongVar
            retorno = confs.getVar(name, "----not found----");
            if (retorno == "----not found----")
            {
                retorno = confs.GetString(confs.getLongVar(out sucess, name));
                usando = false;
                if (!sucess)
                    return defValue;
            }
            usando = false;
            return retorno;
        }
    }
}
