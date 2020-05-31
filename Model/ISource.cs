using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PropertyTool.DataBase;

namespace PropertyTool.Model
{

    public interface ISource
    {
        IAsyncEnumerable<IEnumerable<Property>> GetProperties();
    }

}