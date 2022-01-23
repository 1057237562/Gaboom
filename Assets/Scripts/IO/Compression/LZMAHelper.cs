/****************************
 * 文件名：LZMAHelper.cs;
 * 创建日期：2015/06/22;
 * Author：chenpeng
 * 文件描述：LZMA压缩解压;
 *****************************/

using UnityEngine;
using System.Collections;
using System.IO;
using SevenZip;

public class LZMAHelper 
{
	public static void Compress(string inpath,string outpath,CodeProgress progress)
	{
		SevenZip.Compression.LZMA.Encoder encoder=new SevenZip.Compression.LZMA.Encoder();
		FileStream inputFS=new FileStream(inpath,FileMode.Open);
		FileStream outputFS=new FileStream(outpath,FileMode.Create);
		
		encoder.WriteCoderProperties(outputFS);
		
		outputFS.Write(System.BitConverter.GetBytes(inputFS.Length),0,8);
		
		encoder.Code(inputFS,outputFS,inputFS.Length,-1,progress);
		outputFS.Flush();
		outputFS.Close();
		inputFS.Close();
	}

	public static void DeCompress(string inpath,string outpath,CodeProgress progress)
	{
		SevenZip.Compression.LZMA.Decoder decoder=new SevenZip.Compression.LZMA.Decoder();
		FileStream inputFS=new FileStream(inpath,FileMode.Open);
		FileStream outputFS=new FileStream(outpath,FileMode.Create);
		
		int propertiesSize=SevenZip.Compression.LZMA.Encoder.kPropSize;
		byte[] properties=new byte[propertiesSize];
		inputFS.Read(properties,0,properties.Length);
		
		byte[] fileLengthBytes=new byte[8];
		inputFS.Read(fileLengthBytes,0,8);
		long fileLength=System.BitConverter.ToInt64(fileLengthBytes,0);
		
		decoder.SetDecoderProperties(properties);
		decoder.Code(inputFS,outputFS,inputFS.Length,fileLength,progress);
		outputFS.Flush();
		outputFS.Close();
		inputFS.Close();
	}
}
