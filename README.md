reyna : Icelandic for "try"
=====
An windows ce store and forward library for http post requests.
Reyna will store your requests and post them when there is a valid connection.

## Installation
Reyna is a standard .net class library and can be referenced through add reference in your project.

## Usage


```C#
  	using Reyna;
  	using Reyna.Interfaces;

	// Add any headers if required
	Header[] headers = new Header[]
	{
		new Header("Content-Type", "application/json"),
		new Header("myheader", "header content"),
	
		// gzip content when posting
		new Header("Content-Encoding", "gzip")
	};

	// Create the message to send
	var reynaMessage = new Reyna.Message(new URI("http://server.tosendmessageto.com"), "body of post, probably JSON");
	reynaMessage.Headers.Add(headers);
    
	// Send the message to Reyna
	var reyna = new ReynaService();
	reyna.Put(storeAndForwardMessage);
	
```
## Latest version is 1.0.36

## Contributors
Pair programmed by [Youhana Hana] (https://github.com/youhana-hana) and [Steve Wood](https://github.com/swood).
