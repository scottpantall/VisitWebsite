using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp;

public class Program
{
	public static string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36";

	public static async Task Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("You can't visit a website without a... you know.. website. Provide a webite as a CLI argument for this to work.");
			return;
		}

		var website = args[0];
		Console.WriteLine(website);

		Console.Write("RestClient: ");
		VisitUsingRestClient(website);

		Console.Write("WebRequest: ");
		VisitUsingWebRequest(website);

		Console.Write("HttpClient: ");
		await VisitUsingHttpClient(website);
	}

	private static void VisitUsingRestClient(string website)
	{
		Uri.TryCreate(website, UriKind.Absolute, out var uri);

		var restClient = new RestClient(uri);
		var request = new RestRequest(Method.GET);
		var response = restClient.Execute(request);

		Console.WriteLine($"{response.StatusCode}: {response.StatusDescription}");
		Console.WriteLine(response.IsSuccessful);
	}

	private static void VisitUsingWebRequest(string website)
	{
		Uri.TryCreate(website, UriKind.Absolute, out var uri);

		try
		{
			// Creating the HttpWebRequest
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.MaxServicePointIdleTime = 5000;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			var request = WebRequest.Create(uri) as HttpWebRequest;
			request.Method = "GET";
			request.UserAgent = userAgent;
			request.Headers.Add("Upgrade-Insecure-Requests", "1");
			request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
			request.AllowAutoRedirect = true;

			var response = request.GetResponse() as HttpWebResponse;

			Console.WriteLine($"{response.StatusCode}: {response.StatusDescription}");
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

	}

	private static async Task VisitUsingHttpClient(string website)
	{
		Uri.TryCreate(website, UriKind.Absolute, out var uri);

        using var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var httpClient = new HttpClient(httpClientHandler);

		var request = new HttpRequestMessage(HttpMethod.Get, uri);
		//https://stackoverflow.com/a/61843229/3447523
		request.Headers.Add("User-Agent", userAgent);
        
        var response = await httpClient.SendAsync(request);

		//https://github.com/dotnet/runtime/issues/23801
		if (response.StatusCode == HttpStatusCode.MovedPermanently)
        {
			var redirectRequest = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
			redirectRequest.Headers.Add("User-Agent", userAgent);
			response = await httpClient.SendAsync(redirectRequest);
        }

		Console.WriteLine($"{response.StatusCode}: {response.ReasonPhrase}");
		Console.WriteLine(response.IsSuccessStatusCode);
	}

	//Differences between RestClient and HttpClient
	//https://stackoverflow.com/a/49963611/3447523
}