using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Web;
using System.Xml;
using Kiss.Web.WebDAV.BaseClasses;
using Kiss.Web.WebDAV.Collections;

namespace Kiss.Web.WebDAV
{
	/// <summary>
	/// Static class for internal functions
	/// </summary>
	internal sealed class InternalFunctions
	{
		private static object FileCriticalSection = new object();

		/// <summary>
		/// GetResourceString() english culture
		/// </summary>
		private static CultureInfo m_EnglishCulture = new CultureInfo("en-US");
		private static SortedList<string, string> __mimeList = new SortedList<string, string>();


		private static SortedList<string, string> MimeList
		{
			get
			{
				return InternalFunctions.__mimeList;
			}
		}

		public static string GetMimeType(string extension)
		{
			string _mimeType = "application/octetstream";
			string _ext = extension.ToLower();

			if (InternalFunctions.MimeList.ContainsKey(_ext))
				_mimeType = InternalFunctions.MimeList[_ext];
			else
			{
				Microsoft.Win32.RegistryKey _rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(_ext);
				if (_rk != null && _rk.GetValue("Content Type") != null)
					_mimeType = _rk.GetValue("Content Type").ToString();

				if (_mimeType != null)
					InternalFunctions.MimeList[_ext] = _mimeType;
			}
			
			return _mimeType;
		}


		private InternalFunctions() { }


		/// <summary>
		/// Internal function to validate the enumerator is of type Int32
		/// </summary>
		/// <param name="enumToValidate"></param>
		/// <exception cref="WebDavException">Throw exception if the enumToValidate value is not a valid Int32</exception>
		internal static bool ValidateEnumType(Enum enumToValidate)
		{
			if (enumToValidate.GetTypeCode() != TypeCode.Int32)
				throw new WebDavException(InternalFunctions.GetResourceString("InvalidEnumIntType"));

			return true;
		}


		/// <summary>
		/// Internal function to process response
		/// </summary>
		/// <param name="errorResources"></param>
		/// <returns></returns>
		internal static string ProcessErrorRequest(DavProcessErrorCollection errorResources)
		{
			return ProcessErrorRequest(errorResources, null);
		}


		/// <summary>
		/// Internal function to process response
		/// </summary>
		/// <param name="errorResources"></param>
		/// <param name="appendResponseNode"></param>
		/// <returns></returns>
		internal static string ProcessErrorRequest(DavProcessErrorCollection errorResources, XmlNode appendResponseNode)
		{
			string _errorRequest = "";

			//Build the response 
			using (Stream _responseStream = new MemoryStream())
			{
				XmlTextWriter _xmlWriter = new XmlTextWriter(_responseStream, new UTF8Encoding(false));

				_xmlWriter.Formatting = Formatting.Indented;
				_xmlWriter.IndentChar = '\t';
				_xmlWriter.Indentation = 1;
				_xmlWriter.WriteStartDocument();

				//Set the Multistatus
				_xmlWriter.WriteStartElement("D", "multistatus", "DAV:");

				//Append the errors
				foreach (Enum _errorCode in errorResources.AllResourceErrors)
				{
					foreach (DavResourceBase _resource in errorResources[_errorCode])
					{
						//Open the response element
						_xmlWriter.WriteStartElement("response", "DAV:");
						_xmlWriter.WriteElementString("href", "DAV:", _resource.ResourcePath);
						_xmlWriter.WriteElementString("status", "DAV:", GetEnumHttpResponse(_errorCode));
						//Close the response element section
						_xmlWriter.WriteEndElement();
					}
				}

				if (appendResponseNode != null)
					appendResponseNode.WriteTo(_xmlWriter);

				_xmlWriter.WriteEndElement();
				_xmlWriter.WriteEndDocument();
				_xmlWriter.Flush();

				using (StreamReader _streamReader = new StreamReader(_responseStream, Encoding.UTF8))
				{
					//Go to the begining of the stream
					_streamReader.BaseStream.Position = 0;
					_errorRequest = _streamReader.ReadToEnd();
				}
				_xmlWriter.Close();
			}
			return _errorRequest;
		}


		/// <summary>
		/// Retrieve the HTTP response status
		/// </summary>
		/// <param name="statusCode"></param>
		/// <returns></returns>
		internal static string GetEnumHttpResponse(Enum statusCode)
		{
			string _httpResponse = "";

			switch (InternalFunctions.GetEnumValue(statusCode))
			{
				case 200:
					_httpResponse = "HTTP/1.1 200 OK";
					break;

				case 404:
					_httpResponse = "HTTP/1.1 404 Not Found";
					break;

				case 423:
					_httpResponse = "HTTP/1.1 423 Locked";
					break;

				case 424:
					_httpResponse = "HTTP/1.1 424 Failed Dependency";
					break;

				case 507:
					_httpResponse = "HTTP/1.1 507 Insufficient Storage";
					break;

				default:
					throw new WebDavException(InternalFunctions.GetResourceString("InvalidStatusCode"));
			}

			return _httpResponse;
		}


		/// <summary>
		/// Internal function to retrieve a valid enumerator's value
		/// </summary>
		/// <param name="statusCode"></param>
		/// <returns></returns>
		/// <exception cref="WebDavException">Throw exception if the statusCode value is not a valid Int32</exception>
		internal static int GetEnumValue(Enum statusCode)
		{
			int _enumValue = 0;
			if (ValidateEnumType(statusCode))
				_enumValue = (int)System.Enum.Parse(statusCode.GetType(), statusCode.ToString(), true);

			return _enumValue;
		}

		/// <summary>
		/// Internal function for retrieving the opaque lock token
		/// </summary>
		/// <param name="inputString"></param>
		/// <returns></returns>
		internal static string ParseOpaqueLockToken(string inputString)
		{
			string _opaqueLockToken = "";

			if (inputString != null)
			{
				string _prefixTag = "<opaquelocktoken:";
				int _prefixIndex = inputString.IndexOf(_prefixTag);
				if (_prefixIndex != -1)
				{
					int _endIndex = inputString.IndexOf('>', _prefixIndex);
					if (_endIndex > _prefixIndex)
						_opaqueLockToken = inputString.Substring(_prefixIndex + _prefixTag.Length, _endIndex - (_prefixIndex + _prefixTag.Length));
				}
			}

			return _opaqueLockToken;
		}


		/// <summary>
		/// Retrieve the resource string
		/// </summary>
		/// <param name="resourceName"></param>
		/// <returns></returns>
		internal static string GetResourceString(string resourceName)
		{
			string _resourceString = "";

			ResourceManager _resourceManager = new ResourceManager("Kiss.Web.WebDAV.Resources", Assembly.GetExecutingAssembly());
			if (_resourceManager.GetResourceSet(m_EnglishCulture, true, false) != null)
				_resourceString = _resourceManager.GetString(resourceName, CultureInfo.CurrentUICulture);

			return _resourceString;
		}

		/// <summary>
		/// Retrieve the resource string
		/// </summary>
		/// <param name="resourceName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal static string GetResourceString(string resourceName, params string[] parameters)
		{
			string _resourceString = String.Format(CultureInfo.InvariantCulture, GetResourceString(resourceName), parameters);
			return _resourceString;
		}		
	}
}
