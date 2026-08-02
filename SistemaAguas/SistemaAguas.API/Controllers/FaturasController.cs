using Antlr.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SistemaAguas.API.Controllers
{
    public class FaturasController : ApiController
    {
        SistemaAguasDataContext db = new SistemaAguasDataContext(ConfigurationManager
    .ConnectionStrings["SistemaAguasConnectionString"]
    .ConnectionString);

        // GET api/faturas
        public List<Fatura> Get()
        {
            var list = from fatura in db.Faturas orderby fatura.Id select fatura;

            return list.ToList();
        }

        // GET api/faturas/5
        public IHttpActionResult Get(int id)
        {
            var fatura = db.Faturas.FirstOrDefault(c => c.Id == id);

            if (fatura == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Fatura não encontrada"));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }
        
        // POST api/faturas
        public IHttpActionResult Post([FromBody] Fatura fatura)
        {
            var consumo = db.Consumos.FirstOrDefault(c => c.Id == fatura.ConsumoId);

            if(consumo == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumo não encontrado"));
            }

            var contador = db.Contadors.FirstOrDefault(c => c.Id == consumo.ContadorId);

            if (contador == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado"));
            }

            var cliente = db.Clientes.FirstOrDefault(c => c.Id == contador.ClienteId);

            if (cliente == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Cliente não encontrado"));
            }

            if (db.Faturas.Any(f => f.ConsumoId == consumo.Id))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,"Já existe uma fatura para este consumo."));
            }

            var valorConsumido = consumo.ValorConsumido;

            double Tarifa = 0;

            if(valorConsumido >= 0 && valorConsumido <= 5 )
            {
                Tarifa = 0.3;
            }
            else if (valorConsumido > 5 && valorConsumido <= 15)
            {
                Tarifa = 0.8;
            }
            else if (valorConsumido > 15 && valorConsumido <= 25)
            {
                Tarifa = 1.2;
            }
            else
            {
                Tarifa = 1.6;
            }

            var valorTotal = valorConsumido * Tarifa;

            fatura.ConsumoId = consumo.Id;
            fatura.ContadorId = contador.Id;
            fatura.ClienteId = contador.ClienteId;
            fatura.ValorTotal = valorTotal;
            fatura.DataFatura = fatura.DataFatura;
            fatura.Pago = false;
            fatura.Anulado = false;

            db.Faturas.InsertOnSubmit(fatura);

            try
            {
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        // PUT api/faturas/5
        public IHttpActionResult Put(int id, [FromBody] Fatura faturaAtualizada)
        {
            var fatura = db.Faturas.SingleOrDefault(f => f.Id == id);

            if (fatura == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Fatura não encontrada"));
            }

            fatura.DataFatura = faturaAtualizada.DataFatura;
            fatura.Pago = faturaAtualizada.Pago;
            
            try
            {
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        // DELETE api/faturas/5
        public IHttpActionResult Delete(int id)
        {
            var fatura = db.Faturas.SingleOrDefault(f => f.Id == id);

            if (fatura == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Fatura não encontrada"));
            }

            if (fatura.Pago == true)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotAcceptable, "Esta fatura já foi paga"));
            }

            try
            {
                db.Faturas.DeleteOnSubmit(fatura);
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, e.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }
    }
}