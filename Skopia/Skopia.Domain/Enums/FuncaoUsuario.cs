using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skopia.Domain.Enums
{
    public enum FuncaoUsuario
    {
        /// <summary>
        /// Usuário padrão com acesso básico às funcionalidades do sistema.
        /// Valor 0 é o padrão para o primeiro membro de um enum, mas é explicitamente definido para clareza e persistência.
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Usuário com privilégios elevados, como acesso a relatórios de desempenho,
        /// aprovações ou outras ações administrativas.
        /// </summary>
        Gerente = 1
    }
}
