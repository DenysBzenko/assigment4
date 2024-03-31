using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

class Node
{
    public char Symbol;
    public int Frequency;
    public Node Left;
    public Node Right;
}

class MinHeap
{
    private List<Node> heap;

    public MinHeap()
    {
        heap = new List<Node>();
    }

    public int Count()
    {
        return heap.Count;
    }

    public void Insert(Node node)
    {
        heap.Add(node);
        int i = heap.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].Frequency <= heap[i].Frequency)
                break;

            Node tmp = heap[i];
            heap[i] = heap[parent];
            heap[parent] = tmp;

            i = parent;
        }
    }

    public Node ExtractMin()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("Heap is empty");

        Node minNode = heap[0];
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        Heapify(0);

        return minNode;
    }

    private void Heapify(int i)
    {
        int left = 2 * i + 1;
        int right = 2 * i + 2;
        int smallest = i;

        if (left < heap.Count && heap[left].Frequency < heap[smallest].Frequency)
            smallest = left;
        if (right < heap.Count && heap[right].Frequency < heap[smallest].Frequency)
            smallest = right;

        if (smallest != i)
        {
            Node tmp = heap[i];
            heap[i] = heap[smallest];
            heap[smallest] = tmp;

            Heapify(smallest);
        }
    }
}

class Program
{
    static MinHeap BuildMinHeap(Dictionary<char, int> frequencies)
    {
        MinHeap minHeap = new MinHeap();
        foreach (var pair in frequencies)
        {
            minHeap.Insert(new Node { Symbol = pair.Key, Frequency = pair.Value });
        }
        return minHeap;
    }

    static Node BuildHuffmanTree(Dictionary<char, int> frequencies)
    {
        MinHeap minHeap = BuildMinHeap(frequencies);

        while (minHeap.Count() > 1)
        {
            Node left = minHeap.ExtractMin();
            Node right = minHeap.ExtractMin();

            Node parent = new Node
            {
                Symbol = '\0',
                Frequency = left.Frequency + right.Frequency,
                Left = left,
                Right = right
            };

            minHeap.Insert(parent);
        }

        return minHeap.ExtractMin();
    }

    static Dictionary<char, string> GenerateHuffmanCodes(Node node)
    {
        Dictionary<char, string> codes = new Dictionary<char, string>();
        GenerateHuffmanCodesHelper(node, "", codes);
        return codes;
    }

    static void GenerateHuffmanCodesHelper(Node node, string code, Dictionary<char, string> codes)
    {
        if (node == null)
            return;

        if (node.Left == null && node.Right == null)
            codes[node.Symbol] = code;

        GenerateHuffmanCodesHelper(node.Left, code + "0", codes);
        GenerateHuffmanCodesHelper(node.Right, code + "1", codes);
    }

    static byte[] EncodeText(string text, Dictionary<char, string> huffmanCodes)
    {
        List<string> encodedBits = new List<string>();
        foreach (char symbol in text)
        {
            if (!huffmanCodes.ContainsKey(symbol))
                throw new Exception($"Symbol '{symbol}' does not have Huffman code.");
            encodedBits.Add(huffmanCodes[symbol]);
        }

        string encodedText = string.Join("", encodedBits);
        int numBytes = (encodedText.Length + 7) / 8;
        byte[] result = new byte[numBytes];
        for (int i = 0; i < numBytes; i++)
        {
            string byteString = encodedText.Substring(i * 8, Math.Min(8, encodedText.Length - i * 8));
            result[i] = Convert.ToByte(byteString.PadRight(8, '0'), 2);
        }
        return result;
    }

    static string DecodeText(byte[] encodedBytes, Node huffmanTree)
    {
        StringBuilder decodedText = new StringBuilder();
        Node currentNode = huffmanTree;
        foreach (byte b in encodedBytes)
        {
            for (int i = 7; i >= 0; i--)
            {
                int bit = (b >> i) & 1;
                if (bit == 0)
                    currentNode = currentNode.Left;
                else
                    currentNode = currentNode.Right;

                if (currentNode.Left == null && currentNode.Right == null)
                {
                    decodedText.Append(currentNode.Symbol);
                    currentNode = huffmanTree;
                }
            }
        }
        return decodedText.ToString();
    }

    static void Main()
    {
        string inputFilePath = "https://raw.githubusercontent.com/kse-ua/algorithms/main/res/sherlock.txt";
        string output;
        string outputFilePath = "encoded_text.bin";

        
        string text = new WebClient().DownloadString(inputFilePath);

        
        Dictionary<char, int> frequencies = CountSymbols(text);

        
        Node huffmanTree = BuildHuffmanTree(frequencies);

        
        Dictionary<char, string> huffmanCodes = GenerateHuffmanCodes(huffmanTree);

        
        foreach (var code in huffmanCodes)
            Console.WriteLine($"Symbol: {code.Key}, Code: {code.Value}");

       
        byte[] encodedBytes = EncodeText(text, huffmanCodes);

        
        File.WriteAllBytes(outputFilePath, encodedBytes);
        Console.WriteLine($"Text was encoded and saved in file: {outputFilePath}");

        
        byte[] encodedData = File.ReadAllBytes(outputFilePath);
        string decodedText = DecodeText(encodedData, huffmanTree);
        Console.WriteLine("\nDecoded text:");
        Console.WriteLine(decodedText);
    }

    static Dictionary<char, int> CountSymbols(string text)
    {
        var result = new Dictionary<char, int>();
        foreach (var symbol in text)
        {
            if (result.ContainsKey(symbol))
                result[symbol]++;
            else
                result[symbol] = 1;
        }
        return result;
    }
}

