using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var pattern1 = "^.*-srcxml2json.*$";
        var pattern2 = "^.*-json2srcxml.*$";
        var regex1 = new Regex(pattern1);
        var regex2 = new Regex(pattern2);
        if (regex1.IsMatch(args[0]))
        {
            // 调用src2json方法，将文件名作为参数传递
            if (Directory.Exists(args[1]) || File.Exists(args[1]))
            {
                var files = Directory.GetFiles(args[1], "*.srcxml");
                foreach (var file in files)
                {
                    srcxml2json(args[1] + "\\" + Path.GetFileName(file));
                }
            }
            else
            {
                Console.WriteLine("Wrong Path!");
            }
        }
        else if (regex2.IsMatch(args[0]))
        {
            if (Directory.Exists(args[1]) || File.Exists(args[1]))
            {
                var files = Directory.GetFiles(args[1], "*.json");
                foreach (var file in files)
                {
                    json2srcxml(args[1] + "\\", Path.GetFileName(file));
                }
            }
            else
            {
                Console.WriteLine("Wrong Path!");
            }
        }
        else
        {
            Console.WriteLine("Wrong Args! We only accept args like -srcxml2json or -srcxml2json!");
        }
    }

    public static void srcxml2json(string fileName)
    {
        var matchContext = new List<(String name, string message)>();
        string patternName = "name=\"(.+?)\"";
        string patternText = "text=\"(.+?)\"";
        // 构建一个正则表达式对象
        Regex regexName = new Regex(patternName);
        Regex regexText = new Regex(patternText);
        // 读取文件内容
        using (StreamReader sr = new StreamReader(fileName))
        {
            string line;
            string lastLine = "";
            int linenum = 0;
            while ((line = sr.ReadLine()) != null)
            {
                linenum++;
                // 在每一行中查找匹配的内容
                Match matchText = regexText.Match(line);
                if (matchText.Success)
                {
                    Match matchName = regexName.Match(lastLine);
                    if (matchName.Success)
                    {
                        matchContext.Add((matchName.Groups[1].Value, matchText.Groups[1].Value));
                    }
                    else
                    {
                        matchContext.Add(("", matchText.Groups[1].Value));
                    }
                }

                lastLine = line;
            }
        }
        // 输出匹配的结果
        //foreach ((string name, string text) in matchContext)
        //{
        //    Console.WriteLine($"{name}:{text}");
        //}
        //Console.WriteLine("Line-------");
        //foreach (int lineNumber in matchLineNum)
        //{
        //    Console.WriteLine($"Line{lineNumber}");
        //}

        //输出结果
        var resJson = "[";
        foreach (var item in matchContext)
        {
            var itemJson = $"{{\"name\":\"{item.name}\",\"message\":\"{item.message.Replace(@"\", @"\\")}\"}}";
            resJson += itemJson + ",";
        }

        if (matchContext.Count > 0)
        {
            resJson = resJson.Remove(resJson.Length - 1);
        }

        resJson += "]";
        resJson = resJson.Replace(",", ",\n   ").Replace("}", "\n}");
        //写入文件
        File.WriteAllText(fileName + ".json", resJson);
        Console.WriteLine($"WriteFile {fileName} Done!");
    }

    public static void json2srcxml(string path, string fileName)
    {
        string json = File.ReadAllText(path+fileName, Encoding.UTF8);
        List<JsonFormat> jsonList = JsonSerializer.Deserialize<List<JsonFormat>>(json);
        string originName = fileName.Substring(0, fileName.LastIndexOf(".json"));
        string patternName = @"name=""([^""]*)""";
        string patternText = @"text=""([^""]*)""";
        Regex regexName = new Regex(patternName);
        Regex regexText = new Regex(patternText);
        //读取文件
        using (StreamReader sr = new StreamReader(path+originName))
        {
            string line;
            string result = "";
            int count = 0;
            while ((line = sr.ReadLine()) != null)
            {
                // 在每一行中查找匹配的内容
                Match matchName = regexName.Match(line);
                if (matchName.Success)
                {
                    string oldName = matchName.Groups[1].Value;
                    string newName = jsonList[count].name;
                    string replacedLine = regexName.Replace(line, "name=\"" + newName + "\"");
                    result += replacedLine + "\n";
                    //text nextline
                    line = sr.ReadLine();
                    Match matchText = regexText.Match(line);
                    string oldText = matchText.Groups[1].Value;
                    string newText = jsonList[count].message;
                    string replacedLine2 = regexText.Replace(line, "text=\"" + newText + "\"");
                    result += replacedLine2 + "\n";
                    count++;
                }
                else
                {
                    result += line+"\n";
                }
            }
            //写入文件
            File.WriteAllText(path+originName + ".after.srcxml", result.Replace(@"\\", @"\"));
            Console.WriteLine($"WriteFile {originName}.after.srcxm Done!");
        } 
    }
}

public class JsonFormat
{
    private string? _name;

    public string? name
    {
        get { return _name; }
        set { _name = value; }
    }
    private string? _message;
    public string? message
    {
        get { return _message; }
        set { _message = value; }
    }
}