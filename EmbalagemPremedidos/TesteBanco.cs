using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbalagemPremedidos
{
    class TesteBanco
    {
        int a, b, c;
        public void Teste()
        {
            for (int i = 0; i < 10000; i++)
            {
                using (var db = new Dados())
                {
                    Produto p = new Produto() {
                        CodBarra = "7895672" + a++.ToString(),
                        PesoEmbalagem = b++,
                        Descricao = "Nescau 2.0 Lata 400g" + c++.ToString() };
                    db.Produto.InsertOnSubmit(p);
                    db.SubmitChanges();
                }
            }
        }

        public void Clear()
        {
            using (var db = new Dados())
            {
                var produtos = from p in db.Produto select p;
                db.Produto.DeleteAllOnSubmit(produtos);
                db.SubmitChanges();
            }
        }
    }
}
