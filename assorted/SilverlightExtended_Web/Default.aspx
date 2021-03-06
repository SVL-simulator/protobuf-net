<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SilverlightExtended_Web._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Silverlight Project Test Page </title>
    <style type="text/css">
        html, body
        {
            height: 100%;
            overflow: auto;
        }
        body
        {
            padding: 0;
            margin: 0;
        }
        #silverlightControlHost
        {
            height: 100%;
        }
    </style>

    <script type="text/javascript">

        var slhost = document.getElementById("slHost");

        function GetStatus(id) {
            slhost.content.Silverlight.Future(id);
        }

        function updateElement(elementID, value) {
            document.getElementById(elementID).innerHTML = value;
        }

        function onSilverlightError(sender, args) {

            var appSource = "";
            if (sender != null && sender != 0) {
                appSource = sender.getHost().Source;
            }
            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            var errMsg = "Unhandled Error in Silverlight 2 Application " + appSource + "\n";

            errMsg += "Code: " + iErrorCode + "    \n";
            errMsg += "Category: " + errorType + "       \n";
            errMsg += "Message: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
                errMsg += "File: " + args.xamlFile + "     \n";
                errMsg += "Line: " + args.lineNumber + "     \n";
                errMsg += "Position: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {
                if (args.lineNumber != 0) {
                    errMsg += "Line: " + args.lineNumber + "     \n";
                    errMsg += "Position: " + args.charPosition + "     \n";
                }
                errMsg += "MethodName: " + args.methodName + "     \n";
            }

            throw new Error(errMsg);
        }
    </script>

</head>
<body>
    <!-- Runtime errors from Silverlight will be displayed here.
	This will contain debugging information and should be removed or hidden when debugging is completed -->
    <div id='errorLocation' style="font-size: small; color: Gray;">
    </div>
    <div id="silverlightControlHost">
        <div style="width: 100%; height: 100%;">
            <div>
                <label>
                    Total ProtoBuf Tests Processed:</label>
                <span id='counter'>0</span>
            </div>
            <div>
                <label>
                    Percentage Complete:</label>
                <span id='percentage'>0</span>
            </div>
            <div>
                <label>
                    Start Time:</label>
                <span id='startTime'>0</span>
            </div>
            <div>
                <label>
                    Finish Time:</label>
                <span id='finishTime'>0</span>
            </div>
            <div>
                <label>
                    Total Time (in milliseconds):</label>
                <span id='totalTime'>0</span>
            </div>
            <object id="slHost" data="data:application/x-silverlight," type="application/x-silverlight-2-b2"
                width="100%" height="800px">
                <param name="source" value="/ClientBin/SilverlightExtended.xap" />
                <param name="onerror" value="onSilverlightError" />
                <param name="background" value="white" />
                <a href="http://go.microsoft.com/fwlink/?LinkID=115261" style="text-decoration: none;">
                    <img src="http://go.microsoft.com/fwlink/?LinkId=108181" alt="Get Microsoft Silverlight"
                        style="border-style: none" />
                </a>
            </object>
        </div>
    </div>
</body>
</html>
