//cliente servidor -- prática de redes
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Servidor
{
	public static int numeroDaPorta = 12345;

	public static void arquivo(string requisicao, NetworkStream socketStream){

      try{


     		string[] req = requisicao.Split('\0');
     		byte[] file = File.ReadAllBytes(req[0]);
        socketStream.Write(file, 0, file.Length);
      	//https://msdn.microsoft.com/en-us/library/ms847613.aspx

     	}
     	catch{

     		Console.Write("\n\nO arquivo desejado não está disponível !!!\n\n");
     		byte[] sendBytes = Encoding.ASCII.GetBytes ("O arquivo desejado não está disponível !!!");
        socketStream.Write(sendBytes, 0, sendBytes.Length);
     	}

		//é hora de enviar
      

	}
	public static void Requisicoes(NetworkStream socketStream){

		TcpClient cliente = new TcpClient();
		
		//li o que o cliente me pediu
    cliente.ReceiveBufferSize = 1024;
    cliente.SendBufferSize = 5000 * 1024;
    cliente.NoDelay = true;

    byte[] bytes = new byte[cliente.ReceiveBufferSize]; 

    socketStream.Read(bytes, 0, (int)cliente.ReceiveBufferSize);

    string requisicao = Encoding.ASCII.GetString (bytes); 

    arquivo(requisicao, socketStream);
      	
	}

	public static void iniciaServidor(){

		  Console.Write("O próximo endereço de ip a ser usado deve ser "+ numeroDaPorta);
		  IPAddress enderecoLocal = IPAddress.Parse("127.0.0.1");
		  TcpListener servidor = new TcpListener(enderecoLocal, numeroDaPorta); //faz o bind, defino um listener, esperando o cliente "conectar"
		  servidor.Start(); //começa a ouvir as requisições dos clientes
	   	Socket conexao = servidor.AcceptSocket(); //estabelece a conexão entre cliente e servidor
  		NetworkStream socketStream = new NetworkStream(conexao);
  		Requisicoes(socketStream);
  		servidor.Stop(); //fecha a conexão do servidor

	}
	public static void Main()
	{
		System.Console.WriteLine("Porta " + numeroDaPorta + " está aberta...");
 		//Thread t = new Thread(iniciaServidor);          
 		//t.Start();
    iniciaServidor();
    //t.Abort();
		
	}
}