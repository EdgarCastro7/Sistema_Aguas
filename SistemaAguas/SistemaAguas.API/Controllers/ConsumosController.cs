using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SistemaAguas.API.Controllers
{
    public class ConsumosController : ApiController
    {
        SistemaAguasDataContext db = new SistemaAguasDataContext(ConfigurationManager
    .ConnectionStrings["SistemaAguasConnectionString"]
    .ConnectionString);

        // GET api/consumos
        public List<Consumo> Get()
        {
            var list = from consumo in db.Consumos orderby consumo.Id select consumo;

            return list.ToList();
        }

        // GET api/consumos/5
        public IHttpActionResult Get(int id)
        {
            var consumo = db.Consumos.SingleOrDefault(c => c.Id == id);

            if(consumo != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, consumo));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound));
        }

        // POST api/consumos
        public IHttpActionResult Post([FromBody] Consumo consumo)
        {
            if (consumo == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest,"Dados do consumo inválidos."));
            }

            var contador = db.Contadors.SingleOrDefault(c => c.Id == consumo.ContadorId);

            if (contador == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado"));
            }

            if(!contador.Ativo)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Este contador encontra-se desativado"));
            }

            var cliente = db.Clientes.SingleOrDefault(c => c.Id == contador.ClienteId);

            if (!cliente.Ativo)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Este cliente encontra-se inativo"));
            }

            var leituraDuplicada = db.Consumos.Any(c => c.ContadorId == consumo.ContadorId && c.DataLeitura == consumo.DataLeitura);

            if (leituraDuplicada)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Já existe uma leitura para este contador nesta data."));
            }

            var ultimoConsumo = (from consumoAnterior in db.Consumos
                                 where consumoAnterior.ContadorId == consumo.ContadorId
                                 orderby consumoAnterior.Id descending
                                 select consumoAnterior).FirstOrDefault();

            if (ultimoConsumo == null)
            {
                consumo.LeituraAnterior = 0;
            }
            else
            {
                consumo.LeituraAnterior = ultimoConsumo.LeituraAtual;
            }

            if (consumo.LeituraAtual < consumo.LeituraAnterior)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A leitura atual não pode ser inferior à leitura anterior"));
            }

            consumo.ValorConsumido = consumo.LeituraAtual - consumo.LeituraAnterior;

            db.Consumos.InsertOnSubmit(consumo);

            try
            {
                db.SubmitChanges();

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, consumo));
            }
            catch (Exception)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError));
            }

        }

        // PUT api/consumos/5
        public IHttpActionResult Put(int id, [FromBody] Consumo consumoAtualizado)
        {
            var consumo = db.Consumos.SingleOrDefault(c => c.Id == id);

            if (consumo == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumo não encontrado"));
            }

            consumo.LeituraAtual = consumoAtualizado.LeituraAtual;

            if (consumo.LeituraAtual < consumo.LeituraAnterior)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A leitura atual não pode ser inferior à leitura anterior"));
            }

            consumo.ValorConsumido = consumo.LeituraAtual - consumo.LeituraAnterior;

            var consumosSeguintes = from c in db.Consumos where c.ContadorId == consumo.ContadorId && consumo.Id < c.Id orderby c.Id ascending select c;

            var consumoAnterior = consumo;

            foreach (var cs in consumosSeguintes)
            {
                cs.LeituraAnterior = consumoAnterior.LeituraAtual;
                cs.ValorConsumido = cs.LeituraAtual - cs.LeituraAnterior;
                consumoAnterior = cs;
            }

            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, e.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        // DELETE api/consumos/5
        public IHttpActionResult Delete(int id)
        {
            var consumo = db.Consumos.SingleOrDefault(c => c.Id == id);

            if (consumo == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumo não encontrado"));
            }

            db.Consumos.DeleteOnSubmit(consumo);

            try
            {
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