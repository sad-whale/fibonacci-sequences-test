using common_types.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_types.Services
{
    //интерфейс отправителя сообщения
    public interface INumberSender
    {
        void Send(FibonacciNumber number);
    }
}
