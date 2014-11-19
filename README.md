reyna : Icelandic for "try"
=====
An windows ce store and forward library for http post requests.
Reyna will store your requests and post them when there is a valid connection.

## Installation
Reyna is a standard .net class library and can be referenced through add reference in your project.

## Usage


```C#
  	using System.Net;
  	using Reyna;
  	using Reyna.Interfaces;

	// Add any headers if required
	WebHeaderCollection headers = new WebHeaderCollection();
	headers.Add("Content-Type", "application/json");
        headers.Add("Content-Encoding", "gzip");
        headers.Add("token", "myToken");
	
	// Create the message to send
	var reynaMessage = new Reyna.Message(new URI("http://server.tosendmessageto.com"), "body of post, probably JSON");
	reynaMessage.Headers.Add(headers);
	reynaMessage.Headers.Add("extra header", "extra header");
    
	// Send the message to Reyna
	var reyna = new ReynaService();
	reyna.Put(reynaMessage);
	
```
## Latest version is 1.0.36

## Contributors
Pair programmed by [Youhana Hana] (https://github.com/youhana-hana) and [Steve Wood](https://github.com/swood).
