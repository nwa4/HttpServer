using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core.Validations
{
    public interface IRequestValidator
    {
        WebResponse Validate(WebRequest request);
    }
}
