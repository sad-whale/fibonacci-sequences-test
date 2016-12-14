using common_types.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace common_types.Services
{
    //интерфейс генерации следующего числа
    public interface INumberProcessor
    {
        FibonacciNumber GetNext(FibonacciNumber nubmer);
    }
}
