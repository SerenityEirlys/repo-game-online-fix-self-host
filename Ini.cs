// Decompiled with JetBrains decompiler
// Type: Ini
// Assembly: patch, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A119720F-C0B7-45D9-86DA-F9EDC6B8F920
// Assembly location: D:\steamcrackprojects\saolonkeke.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class Ini
{
  private Dictionary<string, Dictionary<string, string>> ini = new Dictionary<string, Dictionary<string, string>>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
  private string file;

  public Ini(string file)
  {
    this.file = file;
    if (!File.Exists(file))
      return;
    this.Load();
  }

  public void Load()
  {
    string str = File.ReadAllText(this.file);
    Dictionary<string, string> dictionary = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
    this.ini[""] = dictionary;
    string[] separator = new string[1]{ "\n" };
    foreach (var data in ((IEnumerable<string>) str.Split(separator, StringSplitOptions.RemoveEmptyEntries)).Select((t, i) => new
    {
      idx = i,
      text = t.Trim()
    }))
    {
      string text = data.text;
      if (text.StartsWith(";") || string.IsNullOrWhiteSpace(text))
        dictionary.Add(";" + data.idx.ToString(), text);
      else if (text.StartsWith("[") && text.EndsWith("]"))
      {
        dictionary = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
        this.ini[text.Substring(1, text.Length - 2)] = dictionary;
      }
      else
      {
        int length = text.IndexOf("=");
        if (length == -1)
          dictionary[text] = "";
        else
          dictionary[text.Substring(0, length)] = text.Substring(length + 1);
      }
    }
  }

  public string GetValue(string key) => this.GetValue(key, "", "");

  public string GetValue(string key, string section) => this.GetValue(key, section, "");

  public string GetValue(string key, string section, string @default)
  {
    return !this.ini.ContainsKey(section) || !this.ini[section].ContainsKey(key) ? @default : this.ini[section][key];
  }

  public void Save()
  {
    StringBuilder sb = new StringBuilder();
    foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair1 in this.ini)
    {
      if (keyValuePair1.Key != "")
      {
        sb.AppendFormat("[{0}]", (object) keyValuePair1.Key);
        sb.AppendLine();
      }
      foreach (KeyValuePair<string, string> keyValuePair2 in keyValuePair1.Value)
      {
        if (keyValuePair2.Key.StartsWith(";"))
        {
          sb.Append(keyValuePair2.Value);
          sb.AppendLine();
        }
        else
        {
          sb.AppendFormat("{0}={1}", (object) keyValuePair2.Key, (object) keyValuePair2.Value);
          sb.AppendLine();
        }
      }
      if (!this.endWithCRLF(sb))
        sb.AppendLine();
    }
    File.WriteAllText(this.file, sb.ToString());
  }

  private bool endWithCRLF(StringBuilder sb)
  {
    return sb.Length < 4 ? sb[sb.Length - 2] == '\r' && sb[sb.Length - 1] == '\n' : sb[sb.Length - 4] == '\r' && sb[sb.Length - 3] == '\n' && sb[sb.Length - 2] == '\r' && sb[sb.Length - 1] == '\n';
  }

  public void WriteValue(string key, string value) => this.WriteValue(key, "", value);

  public void WriteValue(string key, string section, string value)
  {
    Dictionary<string, string> dictionary;
    if (!this.ini.ContainsKey(section))
    {
      dictionary = new Dictionary<string, string>();
      this.ini.Add(section, dictionary);
    }
    else
      dictionary = this.ini[section];
    dictionary[key] = value;
  }

  public string[] GetKeys(string section)
  {
    return !this.ini.ContainsKey(section) ? new string[0] : this.ini[section].Keys.ToArray<string>();
  }

  public string[] GetSections()
  {
    return this.ini.Keys.Where<string>((Func<string, bool>) (t => t != "")).ToArray<string>();
  }
}
