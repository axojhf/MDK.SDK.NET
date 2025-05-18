using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDK.SDK.NET.Extensions;
/// <summary>
/// Player.Extensions
/// </summary>
public static class PlayerExtensions
{
    /// <summary>
    /// Set UserAgent When Playing Video Stream from URL
    /// </summary>
    /// <param name="player"></param>
    /// <param name="userAgent"></param>
    public static void SetUserAgent(this MDKPlayer player, string userAgent)
    {
        player.SetProperty("user-agent", userAgent);
    }

    /// <summary>
    /// Set Headers When Playing Video Stream from URL
    /// </summary>
    /// <param name="player"></param>
    /// <param name="headers"></param>
    public static void SetHeaders(this MDKPlayer player, Dictionary<string, string> headers)
    {
        var headers_str = new StringBuilder();
        foreach (var item in headers)
        {
            headers_str.Append(item.Key);
            headers_str.Append(": ");
            headers_str.Append(item.Value);
            headers_str.Append("\r\n");
        }
        player.SetProperty("headers", headers_str.ToString());
    }

    /// <summary>
    /// Set Cookies When Playing Video Stream from URL
    /// </summary>
    /// <param name="player"></param>
    /// <param name="cookies"></param>
    public static void SetCookies(this MDKPlayer player, string cookies)
    {
        player.SetProperty("cookies", cookies);
    }

    /// <summary>
    /// Set HttpProxy When Playing Video Stream from URL
    /// </summary>
    /// <param name="player"></param>
    /// <param name="http_proxy"></param>
    public static void SetHttpProxy(this MDKPlayer player, string http_proxy)
    {
        player.SetProperty("http_proxy", http_proxy);
    }

    /// <summary>
    /// Set Http Method When Playing Video Stream from URL
    /// </summary>
    /// <param name="player"></param>
    /// <param name="method"></param>
    public static void SetMethod(this MDKPlayer player, string method)
    {
        player.SetProperty("method", method);
    }
}