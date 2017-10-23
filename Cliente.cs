using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Cliente{


   public static string nomeSite(ref string enderecoServidor){

      if((enderecoServidor[0]=='w')&&(enderecoServidor[3]=='.')){ //www.
  		string[] retorno = enderecoServidor.Split('.');
     	return retorno[1];

      }
      else{

         return "index";

      }

   }

   public static string nomeSaida(ref string pastaRequerida){
   	  int i=0, cont=0;
   	  while(i<pastaRequerida.Length){
   	  		if(pastaRequerida[i]=='/'){
   	  			cont=1;
   	  			break;
   	  		}
   	  		i=i+1;
   	  }
   	  if(cont==1){
   	  	string[] retorno = pastaRequerida.Split('/');
        return retorno[retorno.Length-1];
   	  }
   	  else{
   	  	return pastaRequerida;
   	  }
      

   }
   public static string verificaPagina(ref string resp){
      List<string> erros = new List<string>(new string[] { @"302 Found", @"502 Bad Gateway", @"404 Not Found", @"301 Moved Permanently", @"401 Unauthorized",  @"400 Bad Request",  @"403 Forbidden",  @"500 Internal Server Error", @"504 Gateway Timeout", @"501 Not Implemented", @"502 Bad Gateway", @"503 Service Unavailable" });
      int i=0;
      Regex rgx;
      MatchCollection encontrados;
      while(i<erros.Count){
         rgx = new Regex(erros[i], RegexOptions.IgnoreCase);
         encontrados = rgx.Matches(resp);
         if(encontrados.Count>0){
               return erros[i];
         }
         i=i+1;
      }
      return "true";
      

   }
   public static void conexao(ref string enderecoServidor, ref int numeroDaPorta, ref  TcpClient cliente, ref string pastaRequerida){

      if(pastaRequerida != "GET / \r\n\n"){
         Console.Write("Você requeriu a pasta/arquivo "+pastaRequerida+" !!!");
      }

      cliente.Connect(enderecoServidor, numeroDaPorta); //

      NetworkStream netStream = cliente.GetStream(); 

      //escrita
      byte[] sendBytes = Encoding.ASCII.GetBytes (pastaRequerida);
      netStream.Write(sendBytes, 0, sendBytes.Length);

      //leitura
      cliente.ReceiveBufferSize = 5000 * 1024;
      byte[] bytes = new byte[cliente.ReceiveBufferSize]; 
      netStream.Read(bytes, 0, (int)cliente.ReceiveBufferSize);
      string resposta = Encoding.UTF8.GetString(bytes);
      string[] resp = resposta.Split('\0');

      if(pastaRequerida == "GET / \r\n\n"){ //navegador
         string tipoErro = verificaPagina(ref resp[0]);
         if(tipoErro=="true"){
            string nomeArquivo = nomeSite(ref enderecoServidor);
            System.IO.File.WriteAllText(nomeArquivo, resp[0], Encoding.UTF8);
            Console.Write("\nO arquivo com a página requisitada foi criado! \n");
         }
         else{
            Console.Write("\nOuve um erro na página requisitada "+ tipoErro +" e não foi possível baixar a mesma, por favor verifique a página !!!\n");
         }
      }
      else{ //servidor
         if(resp[0] != "O arquivo desejado não está disponível !!!"){
            string nomeArquivo = nomeSaida(ref pastaRequerida);

            //System.IO.File.WriteAllText(nomeArquivo, resp[0],Encoding.UTF8);
            File.WriteAllBytes(nomeArquivo, bytes);
         }
         else{
            Console.Write("\n"+resp);
         }
         
      }
   }

   public static void tratamentoServidor(ref string pastaRequerida, ref int numeroDaPorta){
      int flag=0;
      Console.WriteLine("Digite o nome da pasta que deseja acessar: ");
      pastaRequerida = Console.ReadLine();
      while ((( numeroDaPorta == 0) || ( numeroDaPorta == 80))){
         if(flag==0){
            Console.WriteLine("Digite o número da porta: ");
         }else{
            Console.WriteLine("O número digitado da porta é inválido, por favor, digite novamente: ");
         }
         numeroDaPorta = Convert.ToInt32(Console.ReadLine());
         flag=1;
      }
   }

   public static void tratamentoNavegador(ref int numeroDaPorta, ref String enderecoServidor){
      Console.WriteLine("Digite o endereço do servidor que deseja acessar: ");
      enderecoServidor = Console.ReadLine();
      enderecoServidor.ToLower();
      numeroDaPorta = 80;
   }

   public static void Main(){

      string enderecoServidor = String.Empty, pastaRequerida = String.Empty;
      int numeroDaPorta = 0;
      Console.WriteLine("Digite o serviço que deseja: (Navegador/Servidor): ");
      string servico = Console.ReadLine();
      if(servico.ToLower()=="servidor"){ //no caso que o cliente esteja tentando fazer um acesso ao servidor
         tratamentoServidor(ref pastaRequerida, ref numeroDaPorta);
         enderecoServidor = "127.0.0.1";
      }
      else if(servico.ToLower()=="navegador"){
         tratamentoNavegador(ref numeroDaPorta, ref enderecoServidor);
         pastaRequerida = "GET / \r\n\n";
      }
      else{
         Console.WriteLine("Opção inválida");
         Environment.Exit(0);
      }

      TcpClient cliente = new TcpClient(); //criando o objeto cliente
      cliente.ReceiveBufferSize =  5000 * 1024;
      cliente.SendBufferSize = 1024;

      try{
         conexao(ref enderecoServidor, ref numeroDaPorta, ref cliente, ref pastaRequerida);
      }
      catch{
         System.Console.WriteLine("\n\n\nA conexão com o servidor " +enderecoServidor+ " não conseguiu ser estabelecida ou foi interrompida no meio, verifique se você está conectado na internet !");

      }
      
      //cliente.Close();// fecha a conexão do cliente

   }
}