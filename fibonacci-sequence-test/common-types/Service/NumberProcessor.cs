using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common_types.Models;

namespace common_types.Services
{
    //реализация генерации нового числа, основанной на хранении предыдущих в словаре
    public class NumberProcessor : INumberProcessor
    {
        private ConcurrentDictionary<string, double> _prevNumbers;

        public NumberProcessor()
        {
            _prevNumbers = new ConcurrentDictionary<string, double>();
        }

        public FibonacciNumber GetNext(FibonacciNumber nubmer)
        {
            var result = new FibonacciNumber() { SequenceId = nubmer.SequenceId };

            if (!_prevNumbers.ContainsKey(nubmer.SequenceId))
            {
                _prevNumbers[nubmer.SequenceId] = 1;
                result.Number = 1;
            }
            else
            {
                result.Number = nubmer.Number + _prevNumbers[nubmer.SequenceId];
                _prevNumbers[nubmer.SequenceId] = result.Number;
            }

            return result;
        }
    }
}
