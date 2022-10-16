using System.Net;
using System.Text;
using System.IO;
using HtmlAgilityPack;
using System.Reflection;
using System;

struct df
{
    public string imgPath;
    public int num;
    public bool err;
}

class Program
{
    df? parseData(string s)
    {
        try
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(s);

            df d = new df();
            d.num = int.Parse(htmlDoc.DocumentNode.SelectSingleNode("//body/div/a").Attributes["href"].Value.Replace("https://xkcd.com/", ""));
            var x = htmlDoc.DocumentNode.SelectSingleNode("//body/div/div/img");
            if (x != null)
            {
                d.imgPath = x.Attributes["src"].Value;
                d.err = false;
            }
            else
            {
                x = htmlDoc.DocumentNode.SelectSingleNode("//body/div/div/a/img");
                if (x != null)
                {
                    d.imgPath = x.Attributes["src"].Value;
                    d.err = false;
                }
                else
                    d.err = true;
            }

            return d;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return null;
    }

    static readonly HttpClient client = new HttpClient();
    async Task<df?> getData(string url)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below
            // string responseBody = await client.GetStringAsync(uri);
            if (response.StatusCode is not HttpStatusCode.OK) return null;
            return parseData(responseBody);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
        return null;
    }

    public async 
    Task
dlTo(string u, string to)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(u);
            response.EnsureSuccessStatusCode();
            // Above three lines can be replaced with new helper method below
            // string responseBody = await client.GetStringAsync(uri);
            if (response.StatusCode is not HttpStatusCode.OK) return;

            var x = await response.Content.ReadAsStreamAsync();
            using (var f = File.OpenWrite(to)) {
                await x.CopyToAsync(f);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    public static async Task<int> Main()
    {
        try
        {
            var asmbly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (asmbly == null)
            {
                Console.WriteLine("The dir is null!");
                return 1;
            }
            var dataDir = Path.Combine(asmbly, "data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            Program p = new Program();
            string urlS = "https://xkcd.com/";
            int appendix = -1;
            Console.WriteLine("DL Comics");

            do
            {
                var d = (await p.getData(urlS + ((appendix is -1 ? "" : appendix.ToString()))));
                if (d?.err != false || d == null) { appendix--; continue; }
                string dl = Path.Combine(dataDir, d.Value.imgPath.Replace("//imgs.xkcd.com/comics/", ""));
                Console.WriteLine("Got Latest: \"{0}\"\nImgPath: \"{1}\"\nDownloading to \"{2}\"\n", d?.num, d?.imgPath, dl);
                await p.dlTo("https:" + d.Value.imgPath, dl);
                appendix = d == null ? -1 : d.Value.num - 1;
            } while (appendix > 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return 0;
    }
}
