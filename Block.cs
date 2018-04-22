using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 
/// </summary>
public class Block
{
    public string Index { get; }
    public DateTime DateTime { get; }
    public string PreviousHash { get; }
    public string CurrentHash { get; }
    public string Data { get; } //json

    private string separator = "|";

    /// <summary>
    /// 
    /// </summary>
    public Block()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="dt"></param>
    /// <param name="prevHash"></param>
    /// <param name="currHash"></param>
    /// <param name="data"></param>
    public Block(string index, DateTime dt, string prevHash, string currHash, string data)
    {
        Index = index;
        DateTime = dt;
        PreviousHash = prevHash;
        CurrentHash = currHash;
        Data = data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public Block NewBlock(string data)
    {
        Block lastBlock = GetLast();

        string newId = lastBlock.IdToHex(HexToId(lastBlock.Index) + 1);
        DateTime dt = DateTime.Now;
        string prevHash = lastBlock.CurrentHash;
        string currHash = CalculateHash(HashSource(newId, dt, prevHash, DataFormat(data)));

        if (string.IsNullOrEmpty(currHash))
        {
            return null;
        }

        return new Block(newId, dt, prevHash, currHash, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private string DataFormat(string data)
    {
        return string.Format("\"Data\": \"{0}\"", data); // {"Data": "<data>"}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string IdToHex(int id)
    {
        return id.ToString("x");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hexId"></param>
    /// <returns></returns>
    public int HexToId(string hexId)
    {
        return int.Parse(hexId, System.Globalization.NumberStyles.HexNumber);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="dt"></param>
    /// <param name="prevHash"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    private string HashSource(string index, DateTime dt, string prevHash, string data)
    {
        return string.Format("{1}{0}{2}{0}{3}{0}{4}", separator, index, dt.ToString("yyyyMMddhhmmss"), prevHash, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public string CalculateHash(string source)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            string hash = GetMd5Hash(md5Hash, source);

            if (VerifyMd5Hash(md5Hash, source, hash))
            {
                return hash;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="md5Hash"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public string GetMd5Hash(MD5 md5Hash, string source)
    {
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
        StringBuilder sBuilder = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="md5Hash"></param>
    /// <param name="source"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    public bool VerifyMd5Hash(MD5 md5Hash, string source, string hash)
    {
        string hashOfSource = GetMd5Hash(md5Hash, source);
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        if (0 == comparer.Compare(hashOfSource, hash))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Block GenesisBlock()
    {
        // 1st block is generated
        string id = IdToHex(0);
        DateTime dt = DateTime.Now;
        string prevHash = CalculateHash(string.Empty);

        if (string.IsNullOrEmpty(prevHash))
        {
            return null;
        }

        string data = DataFormat("Genesis block");
        string currHash = CalculateHash(HashSource(id, dt, prevHash, data));

        if (string.IsNullOrEmpty(currHash))
        {
            return null;
        }

        return new Block(id, dt, prevHash, currHash, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public Block GenerateNew(string data)
    {
        Block lastBlock = GetLast();
        string nextId = IdToHex(HexToId(lastBlock.Index) + 1);
        DateTime dt = DateTime.Now;
        string newCurrHash = CalculateHash(HashSource(nextId, dt, lastBlock.CurrentHash, data));

        if (string.IsNullOrEmpty(newCurrHash))
        {
            return null;
        }

        return new Block(nextId, dt, lastBlock.CurrentHash, newCurrHash, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Block GetLast()
    {
        Block block = null;

        return block;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newBlock"></param>
    /// <param name="prevBlock"></param>
    /// <returns></returns>
    public bool IsValid(Block newBlock, Block prevBlock)
    {
        int prevId = HexToId(prevBlock.Index);
        int newId = HexToId(newBlock.Index);

        if (prevId + 1 != newId)
        {
            return false;
        }

        if (prevBlock.CurrentHash != newBlock.PreviousHash)
        {
            return false;
        }

        string source = HashSource(newBlock.Index, newBlock.DateTime, newBlock.PreviousHash, newBlock.Data);

        if (newBlock.CurrentHash != CalculateHash(source))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chainBlock"></param>
    /// <param name="conflictBlock"></param>
    /// <returns></returns>
    private Block ReplaceChain(Block chainBlock, Block conflictBlock)
    {
        if (chainBlock.CurrentHash.Length > conflictBlock.CurrentHash.Length)
        {
            return chainBlock;
        }

        return conflictBlock;
    }
}
