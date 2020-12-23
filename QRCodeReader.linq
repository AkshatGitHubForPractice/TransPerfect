<Query Kind="Program">
  <NuGetReference>CLAP</NuGetReference>
  <NuGetReference>Microsoft.Net.Http</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>CLAP.Validation</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>CLAP</Namespace>
</Query>

void Main(string[] args)
{  /*

    See CLAP page to learn how to pass parameters.
    https://adrianaisemberg.github.io/CLAP/

    To call this method public static void GetQrCodeDetails

    If QR1.png (QRCode file) is present in C:\QrCodeReader folder
    Following needs to be copy/pasted when running from command line     
    lprun.exe "C:\Users\amistry\Documents\LINQPad Queries\QrCodeReader.linq" GetQrCodeDetails -qrCodeFilePath=C:\QrCodeReader\QR1.png
   */

    if (args != null && args.Length > 0)
    {
        try
        {
            Parser.Run<CommandLine>(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw e;
        }
        return;
    }
    var qrCodeFilaPath = @"C:\QrCodeReader\Transperfect.png";
    CommandLine.GetQrCodeDetails(qrCodeFilaPath);
}

public class CommandLine
{
    [Verb]
    public static void GetQrCodeDetails(string qrCodeFilePath)
    {
        var qrCodeReader = new QrCodeReader(qrCodeFilePath);
        qrCodeReader.GetQrCodeDetails();
    }
}

public class QrCodeReader
{
    private string QrCodeFilePath { get; }
    private string QrCodeDetailsApi
    {
        get
        {
            return "http://api.qrserver.com/v1/read-qr-code/";
        }
    }
        
    private List<string> ValidFileExtension = new List<string>()
    {
        ".png",
        ".jpeg",
        ".gif"        
    };
    
    public QrCodeReader(string qrCodeFilePath)
    {
        QrCodeFilePath = qrCodeFilePath ?? throw new ArgumentNullException(nameof(qrCodeFilePath));
    }

    public void GetQrCodeDetails()
    {
        //check if path exists 
        if (!File.Exists(QrCodeFilePath))
            throw new InvalidOperationException($"QR Code file does not exist at : [{QrCodeFilePath}]. Please pass a valid file path using command line.");

        //Check valid file extensions 
        var qrCodeFileExtension =  Path.GetExtension(QrCodeFilePath);
        if (!ValidFileExtension.Contains(qrCodeFileExtension, StringComparer.OrdinalIgnoreCase))
        {
            var validFileExtensionAsString = string.Join(", ", ValidFileExtension.ToArray());
            throw new InvalidOperationException($"Invalid file extension provided : [{QrCodeFilePath}]. List of valid files extensions are {validFileExtensionAsString}");
        }            

        var responseFromApi = Upload().Result;
        ParseQrCodeApiResponse(responseFromApi);        
    }
    
    public void ParseQrCodeApiResponse (string response)
    {
        //Remove Square Brackets from first and last index to make this a valid json.
        var reponseAsJson = Newtonsoft.Json.Linq.JObject.Parse(response.Substring(1, response.Length - 2))["symbol"][0];
        if (!string.IsNullOrEmpty(reponseAsJson["data"].ToString()))
        {
            reponseAsJson["data"].Dump();
        }
        else
        {
            var errorMessage = reponseAsJson["error"] + $"for the filepath provided : [{QrCodeFilePath}]";
            errorMessage.Dump();
        }
    }    

    private async System.Threading.Tasks.Task<string> Upload()
    {
        try
        {
            var upfileBytes = File.ReadAllBytes(QrCodeFilePath);
            HttpClient client = new HttpClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent baContent = new ByteArrayContent(upfileBytes);
            content.Add(baContent, "file", "QRCode");

            var response = await client.PostAsync(QrCodeDetailsApi, content);
            var resultString = await response.Content.ReadAsStringAsync();
            return resultString;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while getting the response from API : [{QrCodeDetailsApi}] ", ex.Message);
            return null;
        }
    }
}