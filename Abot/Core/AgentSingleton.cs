using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Abot.Core
{
    /// <summary>
    /// 用于存放代理的单例类
    /// 线程安全
    /// </summary>
    public class AgentSingleton
    {
        /// <summary>
        /// 使用volatile关键字保其可见性  
        /// volatile 关键字指示一个字段可以由多个同时执行的线程修改，
        /// 通常用于由多个线程访问但不使用 lock 语句对访问进行序列化的字段。
        /// </summary>
        private volatile static AgentSingleton instance = null;
        /// <summary>
        /// 
        /// </summary>
        private static readonly object syncRoot = new object();
        /// <summary>
        /// 代理序列
        /// </summary>
        private static List<Agenter> agenters = null;
        /// <summary>
        /// 用于循环遍历agenters内部的代理
        /// </summary>
        private static int index = 0;
        /// <summary>
        /// 构造函数
        /// </summary>
        private AgentSingleton()
        {
            agenters = new List<Agenter>();
            //string[] ips = File.ReadAllLines(@"d:\ip.txt");
            //foreach (String ip in ips)
            //{
            //    String[] info = ip.Split(',');
            //    if (info.Length > 3 && info[2].ToLower().IndexOf("http") != -1)
            //    {
            //        int port = 0, survibal = 0;
            //        if (int.TryParse(info[1], out port) && int.TryParse(info[3], out survibal))
            //        {
            //            agenters.Add(new Agenter()
            //            {
            //                ip = ip.Split(',')[0],
            //                port = port,
            //                type = info[2],
            //                survibal = survibal,
            //                usable = true
            //            });
            //        }
            //    }
            //}
            //agenters = (from q in agenters orderby q.survibal descending select q).ToList();
        }
        /// <summary>
        /// 获取单例
        /// </summary>
        public static AgentSingleton getInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new AgentSingleton();
                        }
                    }
                }
                return instance;
            }
        }
        /// <summary>
        /// 获取一个IP地址
        /// </summary>
        /// <returns></returns>
        public Agenter get()
        {
            lock (syncRoot)
            {
                if (agenters == null || agenters.Count == 0)
                    return null;
                if (index >= agenters.Count)
                    index = 0;
                return agenters[index++];
            }
        }
        /// <summary>
        /// 增加一个代理IP
        /// </summary>
        /// <param name="agenter"></param>
        public void add(Agenter agenter)
        {
            lock (syncRoot)
            {
                var exist = (from q in agenters where q.ip == agenter.ip select q).FirstOrDefault();
                if (exist == null && AgentCheck.agentCheck(agenter))
                {
                    agenters.Add(agenter);
                    System.IO.File.AppendAllText("D:\\dazhongip.txt", agenter.ToString()+"\r\n");
                }
                //按存活时间进行倒序排列
                agenters = (from q in agenters orderby q.survibal descending select q).ToList();
            }
        }
        /// <summary>
        /// 移除一个节点
        /// </summary>
        /// <param name="agenter"></param>
        public void delete(Agenter agenter)
        {
            lock (syncRoot)
            {
                var exist = (from q in agenters where q.ip == agenter.ip select q).FirstOrDefault();
                if (exist != null)
                    agenters.Remove(exist);
            }
        }
        /// <summary>
        /// 返回缓存的代理IP的数量
        /// </summary>
        /// <returns></returns>
        public int count() {
            return agenters.Count;
        }
    }
}
