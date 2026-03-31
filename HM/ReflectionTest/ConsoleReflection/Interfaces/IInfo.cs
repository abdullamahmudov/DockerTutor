using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleReflection.Interfaces
{
    public interface IInfo
    {
        /// <summary>
        /// Получает информацию об операционной системе и аппаратном обеспечении.
        /// </summary>
        /// <returns>Строка с информацией об ОС и железе</returns>
        string GetInfo();
    }
}