<%@ Application Language="C#" %>

<script runat="server">
     private const string DummyPageUrl = "http://172.16.252.20:7300/home.aspx";  
    private const string DummyCacheItemKey = "default";  
    void Application_Start(object sender, EventArgs e) 
    {
        System.Net.ServicePointManager.DefaultConnectionLimit = 512;
        //每10分钟执行一次代理IP检索
        QuartzHelp.ExecuteByCron<Ip181Agent>("0 0/10 * * * ? ");
        //每10分钟从西祠执行一次代理IP检索
        QuartzHelp.ExecuteByCron<XiciAgent>("0 0/10 * * * ? ");
        //每30分钟从快代理执行一次代理IP检索
        QuartzHelp.ExecuteByCron<KuaidaiAgent>("0 0/30 * * * ? ");
       
    }
    
     void Application_End(object sender, EventArgs e)
    {
        //  在应用程序关闭时运行的代码

    }

    void Application_Error(object sender, EventArgs e)
    {
        // 在出现未处理的错误时运行的代码

    }

    void Session_Start(object sender, EventArgs e)
    {
        // 在新会话启动时运行的代码

    }

    void Session_End(object sender, EventArgs e)
    {
        // 在会话结束时运行的代码。 
        // 注意: 只有在 Web.config 文件中的 sessionstate 模式设置为
        // InProc 时，才会引发 Session_End 事件。如果会话模式设置为 StateServer
        // 或 SQLServer，则不引发该事件。

    }
    protected void Application_BeginRequest(Object sender, EventArgs e)  
    {  
        if (HttpContext.Current.Request.Url.ToString() == DummyPageUrl)  
        {  
            RegisterCacheEntry();  
        }  
    }  
    // 注册一缓存条目在5分钟内到期，到期后触发的调事件  
    private void RegisterCacheEntry()
    {
        if (null != HttpContext.Current.Cache[DummyCacheItemKey]) return;
        HttpContext.Current.Cache.Add(DummyCacheItemKey, "default", null, DateTime.MaxValue,
            TimeSpan.FromMinutes(5), CacheItemPriority.NotRemovable,
            new CacheItemRemovedCallback(CacheItemRemovedCallback));
    }

    // 缓存项过期时程序模拟点击页面，阻止应用程序结束  
    public void CacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
    {
        HitPage();
    }

    // 模拟点击网站网页  
    private void HitPage()
    {
        System.Net.WebClient client = new System.Net.WebClient();
        client.DownloadData(DummyPageUrl);
    }
       
</script>
