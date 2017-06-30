using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ApStorage
{

    
    class ApRamDB2 : ApDbInterface
    {
        public class nodeData
        {
            public int indexCharComp;
            public nodeData[] childs = new nodeData[16];
            public string hexName;
            public string realName;
            public string valor;
        };

        public int keySize;
        public int dataSize;

        nodeData noRaiz;



        public ApRamDB2(int maxKeySize, int maxDataSize)
        {
            //* 2-> as chaves são convertidas, automaticamente para hexadecimal
            //+ 1-> para que se possa botar o "\0" no final da chave

            keySize = maxKeySize * 2 + 1;

            dataSize = maxDataSize;
        }

        public override bool setVar(string pname, string value)
        {
            _InternalSetAndGetVar(pname, value, true);
            return true;
        }


        public override string getVar(string name, string defaultValue)
        {
            nodeData result = _InternalSetAndGetVar(name, defaultValue, false);
            return result.valor;
        }

        

        nodeData _InternalSetAndGetVar(string hexName, string value, bool setN)
        {
            //converte onome para hexadecimal
            


            nodeData retorno = this.criaNoVazio(true);
            retorno.realName = hexName;
            hexName = string_to_hex(hexName);

            //o nome do retorno e corridigo no final
            retorno.hexName = hexName;
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
                        if (noRaiz == null)
                        {
                            noRaiz = new nodeData();
                        }

                        noAtual = noRaiz;

                        estado = "E_localizaDestino";
                        break;
                    case "E_localizaDestino":

                        //case E_localizaDestino_destinoExiste:*/

                        indiceEndereco = hexName[noAtual.indexCharComp];

                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        if (noAtual.childs[indiceEndereco] != null)
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

                        //case E_preparaVariaveisEVerificaNos_DefineNoDestinoComoNoAtual:*/
                        noAtual = noAtual.childs[indiceEndereco];

                        //estado = E_noAtualEOProcurado;

                        //case E_preparaVariaveisEVerificaNos_noAtualEOProcurado:tino_verificaIndice
                        if (noAtual.hexName == hexName)
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
                        }

                        retorno = noAtual;

                        estado = "E_fim";

                        break;
                    case "E_ChaveNoAtualCabeNaChaveProcurada":
                        contem = true;
                        if (hexName.Length < noAtual.hexName.Length)
                            contem = false;
                        else
                        {
                            for (int cont = 0; cont < noAtual.hexName.Length; cont++)
                            {
                                if (noAtual.hexName[cont] != hexName[cont])
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
                    
                    case "E_criaNoIntermediario":
                        if (!setN)
                        {
                            estado = "E_fim";
                            break;
                        }
                        //case E_criaNoIntermediario_CriaNoIntermediario:
                        noIntermediario = criaNoVazio(true);
                        //define o nome do novo nó
                        tempInt = 0;
                        for (int cont = 0; cont < noAtual.hexName.Length; cont++)
                        {
                            if ((cont < hexName.Length) && (noAtual.hexName[cont] == hexName[cont]))
                            {
                                noIntermediario.hexName += noAtual.hexName[cont];
                                tempInt++;
                            }
                            else
                                break;
                        }
                        noIntermediario.indexCharComp = noIntermediario.hexName.Length;//tempInt;

                        //case E_criaNoIntermediario_ApontaONovoNoParaONoAtual:tino_verificaIndice
                        indiceEndereco = noAtual.hexName[noIntermediario.indexCharComp];

                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        noIntermediario.childs[indiceEndereco] = noAtual;
                        //grava o nó
                        //verifica se é o nó procurado, se for, já seta o valor

                        if (noIntermediario.hexName == hexName)
                            noIntermediario.valor = value;

                        //case E_criaNoIntermediario_ApontaNoPaiParaONovoNo:
                        indiceEndereco = noIntermediario.hexName[noPai.indexCharComp];

                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        noPai.childs[indiceEndereco] = noIntermediario;

                        //verifica se o nó intermediário é o procurado
                        if (noIntermediario.hexName == hexName)
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
                        //define o nome do novo nó
                        noIntermediario.hexName = hexName;
                        noIntermediario.valor = value;
                        noIntermediario.indexCharComp = noIntermediario.hexName.Length;
                        //grava o nó
                        
                        //case E_criaNovoNo_ApontaNoAtualParaONovoNo:*/
                        indiceEndereco = noIntermediario.hexName[noAtual.indexCharComp];

                        if ((indiceEndereco >= '0') && (indiceEndereco <= '9'))
                            indiceEndereco -= 48;
                        else
                            indiceEndereco -= 55;

                        noAtual.childs[indiceEndereco] = noIntermediario;

                        
                        retorno = noIntermediario;
                        estado = "E_fim";

                        break;
                    case "E_fim":
                        //verifica se deve fechar o arquivo
                        estado = "E_sair";
                        break;
                }
            }


            retorno.realName = hex_to_string(retorno.hexName);

            return retorno;
        }

        public override bool setLongVar(byte[] data, string name)
        {
            this.setVar(name, GetString(data));
            return true;
        }

        public override byte[] getLongVar(out bool result, string name)
        {

            string valor = this.getVar(name, "---- not found ------==//\\\\");
            if (valor != "---- not found ------==//\\\\")
            {
                result = true;
                return GetBytes(valor, false, 0);
            }
            else
            {
                result = false;
                return GetBytes("", false, 0);
            }
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
                realName = "",
                hexName = "",
                valor = ""
            };

            return temp;
        }

        
    }
}
