using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ApStorage
{

    
    class ApFileDB2 : ApDbInterface
    {
        public class nodeData
        {
            public int indexCharComp;
            public long[] childs = new long[] { long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue };
            public string nome;
            public byte[] valor;
            public long posInFile;
        };
        
        
        protected string filename;
        protected System.IO.FileStream arq;
        protected bool exclusiveUse = false;
        protected bool opened;
        protected bool verificaIntegridade;

        public int keySize;
        public int dataSize;



        public ApFileDB2(int maxKeySize, int maxDataSize, bool exclusiveFileUse, string fName, bool pVerificaIntegridade)
        {
            verificaIntegridade = pVerificaIntegridade;
            //ctor
            opened = false;
            //* 2-> as chaves são convertidas, automaticamente para hexadecimal
            //+ 1-> para que se possa botar o "\0" no final da chave

            keySize = maxKeySize * 2 + 1;

            dataSize = maxDataSize;
            exclusiveUse = exclusiveFileUse;

            filename = fName;
        }



        public override bool setVar(string pname, string value)
        {
            _InternalSetAndGetVar(pname, GetBytes(value, true, dataSize), true);
            return true;
        }

        
        public override string getVar(string name, string defaultValue)
        {
            nodeData result = _InternalSetAndGetVar(name, GetBytes(defaultValue, true, 0), false);
            return GetString(result.valor);
        }

           public string fileheader = "APFILEDB1.0.0.0";
        nodeData _InternalSetAndGetVar(string name, byte[] value, bool setN)
        {
            //converte onome para hexadecimal
            name = string_to_hex(name);
            

            nodeData retorno = this.criaNoVazio(true);

            //o nome do retorno e corridigo no final
            retorno.nome = name;
            retorno.valor = value;

            string estado = "E_inicio";
            nodeData noAtual = null;
            nodeData noPai = null;
            nodeData noIntermediario = null;

            int indiceEndereco = -1;

            bool contem;

            int tempInt;

            

            while (estado != "E_sair")
            {
                switch (estado)
                {
                    case "E_inicio":
                        estado = "E_preparaNoRaiz";
                        break;
                    case "E_preparaNoRaiz":
                    //case E_pE_preparaVariaveisEVerificaNos_noAtualEOProcuradoreparaNoRaiz_NoRaizExiste:*/
                        if ((!opened) && (!System.IO.File.Exists(filename)))
                        {
                            //%%%%%%%%% testar abertura, para permitir acesso concorente
                            arq = new System.IO.FileStream(filename, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);//System.IO.File.Open(filename, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);

                            opened = true;
                            arq.Seek(0, System.IO.SeekOrigin.Begin);
                            this.criaCabecalho();

                            noAtual = this.criaNoVazio(true);
                            noAtual.indexCharComp = 0;
                            noAtual.posInFile = 0;
                            //zera as variáveis

                            noAtual.posInFile = fileheader.Length;
                            this.gravaUmNo(noAtual);
                        }
                    
                        if (!opened)
                        {
                            //%%%%%%%%% testar abeE_alteraValorDoNoAtual;rtura, para permitir acesso concorente
                            arq = new System.IO.FileStream(filename, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);//(filename, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
                            arq.Seek(0, System.IO.SeekOrigin.Begin);
                            opened = true;

                            if (!this.verificaCabecalho(true))
                            {
                                //throw APError("Invalid or corrupt file");
                                return retorno;
                            }
                        }

                        arq.Seek(fileheader.Length, System.IO.SeekOrigin.Begin);
                        noAtual = this.leUmNo();

                        estado = "E_localizaDestino";
                        break;
                    case "E_localizaDestino":

                    //case E_localizaDestino_destinoExiste:*/

                        indiceEndereco = name[noAtual.indexCharComp];
                         
                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        if (noAtual.childs[indiceEndereco] != long.MaxValue)
                        {
                            estado = "E_preparaVariaveisEVerificaNos";
                        }
                        else
                        {
                            estado = "E_criaNovoNo";
                        }
                        break;
                    case "E_preparaVariaveisEVerificaNos":

                    //case E_preparaVariaveisEVerificaNos_salvaNoAtualEmNoPai :*/
                        noPai = noAtual;
                        arq.Seek(noAtual.childs[indiceEndereco], System.IO.SeekOrigin.Begin);

                    //case E_preparaVariaveisEVerificaNos_DefineNoDestinoComoNoAtual:*/
                        noAtual = this.leUmNo();

                        //estado = E_noAtualEOProcurado;

                    //case E_preparaVariaveisEVerificaNos_noAtualEOProcurado:tino_verificaIndice
                        if (noAtual.nome == name)
                        {
                            estado = "E_alteraValorDoNoAtual";
                        }
                        else
                        {
                            estado = "E_ChaveNoAtualCabeNaChaveProcurada";
                        }
                        break;
                    case "E_alteraValorDoNoAtual":
                        if (setN)
                        {
                            noAtual.valor = value;
                            arq.Seek(noAtual.posInFile, System.IO.SeekOrigin.Begin);
                            this.gravaUmNo(noAtual);
                        }

                        retorno = noAtual;

                        estado = "E_fim";

                        break;
                    case "E_ChaveNoAtualCabeNaChaveProcurada":
                        contem = true;
                        if (name.Length < noAtual.nome.Length)
                            contem = false;
                        else
                        {
                            for (int cont = 0; cont < noAtual.nome.Length; cont++)
                            {
                                if (noAtual.nome[cont] != name[cont])
                                {
                                    contem = false;
                                    break;
                                }
                            }
                        }

                        if (contem)
                        {
                            //verifica se o tamanho da chave procurada é menor que o indice de comparação no nó atual
                            /*if (name.Length - 1 < noAtual.indexCharComp)
                                estado = "E_MoveNoAtualParaTraz";
                            else*/
                                estado = "E_localizaDestino";
                        }
                        else
                        {
                            estado = "E_criaNoIntermediario";
                        }

                        break;
                   /* case "E_MoveNoAtualParaTraz":
                        if (!setN)
                        {
                            estado = "E_fim";
                            break;
                        }
                        //cria um novo nó com o nome e valor da chave procurada
                            //utiliza a variavel noIntermediario como variavel temporaia
                        noIntermediario = this.criaNoVazio(true);
                        noIntermediario.nome = name;
                        noIntermediario.valor = value;
                        arq.Seek(0, System.IO.SeekOrigin.End);
                        noIntermediario.posInFile = arq.Position;
                        //copia o índice e os dilhos do nó atual para o novo nó
                        noIntermediario.indexCharComp = noAtual.indexCharComp;
                        noIntermediario.childs = noAtual.childs;
                        //zera os filhos do nó atual
                        for (int cont = 0; cont < noAtual.childs.Length; cont++)
                            noAtual.childs[cont] = long.MaxValue;

                        //redefine o indice de comparação do no atual (tamanho do nome procurado-1)
                        noAtual.indexCharComp = name.Length - 1;
                        //aponda o nó atual para o novo nó 
                        indiceEndereco = name[noAtual.indexCharComp];
                         
                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        noAtual.childs[indiceEndereco] = noIntermediario.posInFile;
                        //grava o novo no
                        gravaUmNo(noIntermediario);
                        //altera o nó atual
                        arq.Seek(noAtual.posInFile, System.IO.SeekOrigin.Begin);
                        gravaUmNo(noAtual);

                        estado = "E_fim";



                            break;*/
                    case "E_criaNoIntermediario":
                        if (!setN)
                        {
                            estado = "E_fim";
                            break;
                        }
                    //case E_criaNoIntermediario_CriaNoIntermediario:
                        noIntermediario = criaNoVazio(true);
                        arq.Seek(0, System.IO.SeekOrigin.End);
                        noIntermediario.posInFile = arq.Position;
                        //define o nome do novo nó
                        tempInt = 0;
                        for (int cont = 0; cont< noAtual.nome.Length; cont++)
                        {
                            if ((cont < name.Length) && (noAtual.nome[cont] == name[cont]))
                            {
                                noIntermediario.nome += noAtual.nome[cont];
                                tempInt++;
                            }
                            else
                                break;
                        }
                        noIntermediario.indexCharComp = noIntermediario.nome.Length;//tempInt;

                    //case E_criaNoIntermediario_ApontaONovoNoParaONoAtual:tino_verificaIndice
                        indiceEndereco = noAtual.nome[noIntermediario.indexCharComp];

                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        noIntermediario.childs[indiceEndereco] = noAtual.posInFile;
                        //grava o nó
                        //verifica se é o nó procurado, se for, já seta o valor

                        if (noIntermediario.nome == name)
                            noIntermediario.valor = value;

                            this.gravaUmNo(noIntermediario);
                    //case E_criaNoIntermediario_ApontaNoPaiParaONovoNo:
                        indiceEndereco = noIntermediario.nome[noPai.indexCharComp];

                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        noPai.childs[indiceEndereco] = noIntermediario.posInFile;

                        arq.Seek(noPai.posInFile, System.IO.SeekOrigin.Begin);
                        this.gravaUmNo(noPai);

                        //verifica se o nó intermediário é o procurado
                        if (noIntermediario.nome == name)
                        {
                            estado = "E_fim";

                        }
                        else
                        {
                            noAtual = noIntermediario;
                            estado = "E_criaNovoNo";
                        }
                        break;
                    case "E_criaNovoNo":
                        if (!setN)
                        {
                            estado = "E_fim";
                            break;
                        }

                    //case E_criaNovoNo_CriaNovoNo:
                        //utiliza a variável noIntermediario para criar o novo no

                        noIntermediario = criaNoVazio();
                        arq.Seek(0, System.IO.SeekOrigin.End);
                        noIntermediario.posInFile = arq.Position;
                        //define o nome do novo nó
                        noIntermediario.nome = name;
                        noIntermediario.valor = value;
                        noIntermediario.indexCharComp = noIntermediario.nome.Length;
                        //grava o nó
                        this.gravaUmNo(noIntermediario);

                    //case E_criaNovoNo_ApontaNoAtualParaONovoNo:*/
                        indiceEndereco = noIntermediario.nome[noAtual.indexCharComp];

                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        noAtual.childs[indiceEndereco] = noIntermediario.posInFile;

                        arq.Seek(noAtual.posInFile, System.IO.SeekOrigin.Begin);
                        this.gravaUmNo(noAtual);

                        retorno = noIntermediario;
                        estado = "E_fim";

                        break;
                    case "E_fim":
                        //verifica se deve fechar o arquivo
                        if (!exclusiveUse)
                        {
                            arq.Close();
                            opened = false;
                        }
                        estado = "E_sair";
                        break;
                }
            }


            retorno.nome =hex_to_string(retorno.nome);

            return retorno;
        }

        public override bool setLongVar(byte[] data, string name)
        {
            byte[] gravar1 = new byte[dataSize];
            int contBlocos, cont, contPercoridos;

            //grava o tamanho do bloco de dados
            this.setVar(name + "._length", data.Length.ToString());

            //grava o tota de blocos
            int blocos = (int)(data.Length / dataSize)+1;

            this.setVar(name + "._count", blocos.ToString());

            //grava os blocos
            cont = 0;
            contBlocos = 0;
            contPercoridos = 0;

            while (contPercoridos < data.Length)
            {
                //prepara o nome da variável

                for (cont = 0; cont < dataSize && contPercoridos < data.Length; cont++)
                    gravar1[cont] = data[contPercoridos++];

                this._InternalSetAndGetVar(name + "._" + contBlocos.ToString(), gravar1, true);
                contBlocos++;
            }
            return true;

        }

        public override byte[] getLongVar(out bool result, string name)
        {
            byte[] gravar1 = new byte[dataSize];
            int gravados = 0;
            int contBlocos, cont, contPercoridos;
            int blocos;

            int dSize;


            //pega o tamanho do bloco de dados
            dSize = System.Convert.ToInt32(this.getVar(name+"._length", "0"));
            if (dSize == 0)
            {
                result = false;
                return new byte[] { 0 };
            }


            //pega o  o tota de blocos
            blocos = System.Convert.ToInt32(this.getVar(name+"._count", "0"));

            //pega os blocos
            cont = 0;
            contBlocos = 0;
            contPercoridos = 0;

            byte[] buffeVazio = new byte[0];
            byte[] buffer;
            byte[] retorno = new byte[dSize];
            nodeData tempData;
            while (contPercoridos < dSize)
            {
                //lê um bloco do arquivo
                tempData = this._InternalSetAndGetVar(name + "._" + contBlocos.ToString(), buffeVazio, false);
                buffer = tempData.valor;

                for (cont = 0; cont < dataSize && contPercoridos < dSize; cont++)
                    retorno[contPercoridos++] = buffer[cont];

                contBlocos++;
            }

            result = true;
            return retorno;
        }

        void criaCabecalho(bool autoseek = false)
        {
            if (autoseek)
                arq.Seek(0, System.IO.SeekOrigin.Begin);

            arq.Write(GetBytes(fileheader, false, 0), 0, fileheader.Length);
        }

        bool verificaCabecalho(bool autoseek = false)
        {
            if (!verificaIntegridade)
                return true;

            if (autoseek)
                arq.Seek(0, System.IO.SeekOrigin.Begin);

            byte[] buffer = new byte[fileheader.Length];
            arq.Read(buffer, 0, fileheader.Length);
            return GetString(buffer) == fileheader;
        }

        string string_to_hex(string input)
        {
            string hex = "";
            foreach (char c in input)
            {
            int tmp = c;
            hex += string.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex.ToUpper();
        }

        string hex_to_string(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
            StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
            HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            return StrValue;
        }


        nodeData criaNoVazio(bool zeraDados = false)
        {
            zeraDados = true;
            nodeData temp = new nodeData
            {
                indexCharComp = -1,
                nome = "",
                posInFile = long.MaxValue,
                valor = new byte[dataSize]
            };
            
            
            if (zeraDados)
            {
                for (int cont = 0; cont < dataSize; cont++)
                    temp.valor[cont] = 0;
            }

            return temp;
        }

       // bool continuar = true;
        byte[] leBuffer(int count)
        {

            System.IO.BinaryReader temp = new System.IO.BinaryReader(arq, Encoding.ASCII);
            
            
            byte[] a = new byte[count];

            while (true)
            {
                try
                {
                    temp.Read(a, 0, count);
                    break;
                }
                catch { System.Threading.Thread.Sleep(1);}
            }
            //arq.ReadAsync(a, 0, count, new System.Threading.CancellationToken(true));*/
            return a;
        }

        void gravaBuffer(byte[] buf, int count)
        {
            System.IO.BinaryWriter temp = new System.IO.BinaryWriter(arq, Encoding.ASCII);
            while (true)
            {
                try
                {
                    temp.Write(buf, 0, count);
                    break;
                }
                catch { System.Threading.Thread.Sleep(1); }
            }
            
            //arq.BeginWrite(buf, 0, count, ReadCompleted, null);
            //arq.WriteAsync(buf, 0, count);
        }

        private void ReadCompleted(IAsyncResult iResult)
        {
            //State tempState = (State)asyncResult.AsyncState;
            int readCount = arq.EndRead(iResult);

            /*int i = 0;
            while (i < readCount)
            {
                if (tempState.ReadArray[i] != tempState.WriteArray[i++])
                {
                    Console.WriteLine("Error writing data.");
                    tempState.FStream.Close();
                    return;
                }
            }
            Console.WriteLine("The data was written to {0} and verified.",
                tempState.FStream.Name);
            tempState.FStream.Close();

            // Signal the main thread that the verification is finished.
            tempState.ManualEvent.Set();*/

            /*if (iResult.IsCompleted)
                continuar = true;*/
            
        }

        nodeData leUmNo()
        {
            
            nodeData retorno = this.criaNoVazio();
            byte[] buffer;
            int cont;
            
            
            retorno.posInFile = arq.Position;
           
            /*//indexCharComp
            buffer = new byte[sizeof(int)];

            buffer = leBuffer(sizeof(int));

            retorno.indexCharComp = System.BitConverter.ToInt32(buffer, 0);
            int cont;

            //childs
            buffer = new byte[sizeof(long)];
            System.Threading.Thread.Sleep(100);
            long pos = arq.Position;
            for (cont = 0; cont < retorno.childs.Length; cont++)
            {
                buffer = leBuffer(sizeof(long));
                retorno.childs[cont] = System.BitConverter.ToInt64(buffer, 0);

                //algumas vezes não lê, então tenta denovo
                if (retorno.childs[cont] == 0)
                {
                    arq.Seek(arq.Position - sizeof(long), System.IO.SeekOrigin.Begin);
                    buffer = leBuffer(sizeof(long));
                    
                    //retorno.childs[cont] = (long)((IntPtr)(buffer));//System.BitConverter.ToInt64(buffer, 0);

                    retorno.childs[cont] = buffer[0] + buffer[1] * 255 + buffer[2] * 65535 + buffer[3] * 16581375 + buffer[4] * 4228250625 + buffer[5] * 1078203909375 + buffer[6] * 274941996890625 + buffer[7] * 70110209207109375;

                }


                if (retorno.childs[cont] == 0)
                    retorno.childs[cont] = retorno.childs[cont] * 1;
            }

            //nome
            buffer = new byte[keySize];
            buffer = leBuffer(keySize);
            for (cont =0; cont < keySize && buffer[cont] != 0; cont++)
                retorno.nome += System.Convert.ToChar(buffer[cont]);
            
            //valor
            retorno.valor = new byte[dataSize];
            retorno.valor = leBuffer(dataSize);
            */

            //System.Threading.Thread.Sleep(1000);
            buffer = leBuffer(sizeof(int) + (16 * sizeof(long)) + keySize + dataSize);

            //pega o caractere a comparar
            retorno.indexCharComp = System.BitConverter.ToInt32(buffer.Skip(0).Take(4).ToArray(), 0);
            //pega os 16 filhos
            for (cont = 0; cont < 16; cont++)
                retorno.childs[cont] = System.BitConverter.ToInt64(buffer.Skip(cont * sizeof(long) + 4).Take(sizeof(long)).ToArray(), 0);
            //pega o nome
            byte[] tempBuffer = buffer.Skip(4 + 16 * sizeof(long)).Take(keySize).ToArray();
            for (cont = 0; cont < keySize && tempBuffer[cont] != 0; cont++)
                retorno.nome += System.Convert.ToChar(tempBuffer[cont]);

            //pega o valor
            retorno.valor = buffer.Skip(4 + 16 * sizeof(long) + keySize).Take(dataSize).ToArray();

            return retorno;

        }

        bool gravaUmNo(nodeData node, bool autoSeekTo = false)
        {
            if (autoSeekTo)
                arq.Seek(node.posInFile, System.IO.SeekOrigin.Begin);

            int cont;
            byte[] buffer;

            buffer = buffer = System.BitConverter.GetBytes(node.indexCharComp);
            gravaBuffer(buffer, sizeof(int));
            //grava os filhos
            for (cont = 0 ; cont < node.childs.Length; cont++)
            {
                buffer = System.BitConverter.GetBytes(node.childs[cont]);
                gravaBuffer(buffer, sizeof(long));
            }

            //grava a chave
            buffer = new byte[keySize];
            for (cont = 0; cont < keySize && cont < node.nome.Length; cont++)
                buffer[cont] = System.Convert.ToByte(node.nome[cont]);

            buffer[cont] = 0;
            gravaBuffer(buffer, keySize);
            //grava o valor
            gravaBuffer(node.valor, dataSize);

            return true;
        }
    }
}
