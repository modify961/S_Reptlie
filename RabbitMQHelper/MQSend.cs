using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQHelper
{
    /// <summary>
    /// 向MQ服务器发送一条消息
    /// </summary>
    public class MQSend
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="message">要发送的数据</param>
        /// <returns></returns>
        public bool send(String queueName,String message) {
            try
            {
                //创建一个ConnectionFactory类，
                //IModel: 表示一个符合AMQP 0 - 9 - 1 协议的通道，并且提供了很多的操作方法
                //IConnection:表示一个符合AMQP 0 - 9 - 1协议的连接对象，用户和RabbitMQ 服务端的连接
                //ConnectionFactory: 可以创建一个IConnection对象的实例。
                //IBasicConsumer: 表示一个消息的消费者，或者是使用者。
                var factory = new ConnectionFactory();
                factory.HostName = "47.96.146.22";
                factory.UserName = "ljw";
                factory.Password = "li809731496";
                factory.Port = 5672;
                //创建一个IConnection的类
                using (var connection = factory.CreateConnection())
                {
                    //创建一个IModel协议通道
                    using (var channel = connection.CreateModel())
                    {
                        /*
                         *在MQ上定义一个持久化队列，如果名称相同不会重复创建
                         * 
                         * 
                         */
                        channel.QueueDeclare(queue: queueName,//队列名
                                             durable: false,//是否持久化
                                             exclusive: false,//排它性
                                             autoDelete: false,//一旦客户端连接断开则自动删除queue
                                             arguments: null);//如果安装了队列优先级插件则可以设置优先级
                        var body = Encoding.UTF8.GetBytes(message);

                        //设置消息持久化
                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = 2;
                        channel.BasicPublish(exchange: "",//exchange名称
                                             routingKey: queueName,//如果存在exchange,则消息被发送到名称为hello的queue的客户端
                                             basicProperties: properties,
                                             body: body);//消息体
                    }

                }
                return true;
            }
            catch (Exception ex) {
                throw (ex);
            }
        }
    }
}
