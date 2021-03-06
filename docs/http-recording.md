<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /docs/mdsource/http-recording.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# Http Recording

Http Recording allows, when a method is being tested, for any http requests made as part of that method call to be recorded and verified.


## Usage

Call `HttpRecording.StartRecording();` before the method being tested is called.

The perform the verification as usual:

<!-- snippet: HttpRecording -->
<a id='snippet-httprecording'></a>
```cs
[Fact]
public async Task TestHttpRecording()
{
    HttpRecording.StartRecording();

    var sizeOfResponse = await MethodThatDoesHttpCalls();

    await Verifier.Verify(
            new
            {
                sizeOfResponse,
            })
        //scrub some headers that are no consistent between test runs
        .ScrubLinesContaining("AGE", "Server", "Date", "Etag");
}

static async Task<int> MethodThatDoesHttpCalls()
{
    using var client = new HttpClient();

    var exampleResult = await client.GetStringAsync("https://example.net/");
    var httpBinResult = await client.GetStringAsync("https://httpbin.org/");
    return exampleResult.Length + httpBinResult.Length;
}
```
<sup><a href='/src/Verify.Tests/Tests.cs#L27-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-httprecording' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The requests/response pairs will be appended to the verified file.

<!-- snippet: Tests.TestHttpRecording.verified.txt -->
<a id='snippet-Tests.TestHttpRecording.verified.txt'></a>
```txt
{
  target: {
    sizeOfResponse: 10847
  },
  httpRequests: [
    {
      Uri: https://example.net/,
      Status: RanToCompletion,
      RequestHeaders: {},
      ResponseHeaders: {
        Vary: Accept-Encoding,
        X-Cache: HIT
      }
    },
    {
      Uri: https://httpbin.org/,
      Status: RanToCompletion,
      RequestHeaders: {},
      ResponseHeaders: {
        Access-Control-Allow-Credentials: true,
        Access-Control-Allow-Origin: *,
        Connection: keep-alive,
      }
    }
  ]
}
```
<sup><a href='/src/Verify.Tests/Tests.TestHttpRecording.verified.txt#L1-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.TestHttpRecording.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Explicit Usage

The above usage results in the http calls being automatically added snapshot file. Calls can also be explicitly read and recorded using `HttpRecording.FinishRecording()`. This enables:

 * Filtering what http calls are included in the snapshot.
 * Only verifying a subset of information for each http call.
 * Performing additional asserts on http calls.

For example:

<!-- snippet: HttpRecordingExplicit -->
<a id='snippet-httprecordingexplicit'></a>
```cs
[Fact]
public async Task TestHttpRecordingExplicit()
{
    HttpRecording.StartRecording();

    var sizeOfResponse = await MethodThatDoesHttpCalls();

    var httpCalls = HttpRecording.FinishRecording().ToList();

    // Ensure all calls finished in under 2 seconds
    var threshold = TimeSpan.FromSeconds(2);
    foreach (var call in httpCalls)
    {
        Assert.True(call.Duration < threshold);
    }

    await Verifier.Verify(
            new
            {
                sizeOfResponse,
                // Only use the Uri in the snapshot
                httpCalls = httpCalls.Select(_ => _.Uri)
            });
}
```
<sup><a href='/src/Verify.Tests/Tests.cs#L56-L83' title='Snippet source file'>snippet source</a> | <a href='#snippet-httprecordingexplicit' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Results in the following:

<!-- snippet: Tests.TestHttpRecordingExplicit.verified.txt -->
<a id='snippet-Tests.TestHttpRecordingExplicit.verified.txt'></a>
```txt
{
  sizeOfResponse: 10847,
  httpCalls: [
    https://example.net/,
    https://httpbin.org/
  ]
}
```
<sup><a href='/src/Verify.Tests/Tests.TestHttpRecordingExplicit.verified.txt#L1-L7' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.TestHttpRecordingExplicit.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
