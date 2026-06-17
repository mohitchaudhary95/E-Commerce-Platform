using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Shared.RabbitMQ.Settings
{
    public class RabbitMQSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
    }
}
