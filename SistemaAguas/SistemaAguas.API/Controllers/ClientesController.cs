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
    public class ClientesController : ApiController
    {
        SistemaAguasDataContext db = new SistemaAguasDataContext(ConfigurationManager
            .ConnectionStrings["SistemaAguasConnectionString"]
            .ConnectionString);

        // GET api/clientes
        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok(db.Clientes.ToList());
        }

        // GET api/clientes/5
        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            var cliente = db.Clientes.SingleOrDefault(c => c.Id == id);

            if (cliente != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, cliente));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Cliente não encontrado"));
        }

        // POST api/clientes
        [HttpPost]
        public IHttpActionResult Post([FromBody] Cliente novoCliente)
        {
            novoCliente.Ativo = true;
            db.Clientes.InsertOnSubmit(novoCliente);
            db.SubmitChanges();

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, novoCliente));
        }

        // PUT api/clientes
        [HttpPut]
        public IHttpActionResult Put(int id, [FromBody] Cliente clienteAtualizado)
        {
            var cliente = db.Clientes.SingleOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Cliente não encontrado"));
            }

            cliente.Nome = clienteAtualizado.Nome;
            cliente.NIF = clienteAtualizado.NIF;
            cliente.Email = clienteAtualizado.Email;
            cliente.CodigoPostal = clienteAtualizado.CodigoPostal;
            cliente.Contacto = clienteAtualizado.Contacto;
            cliente.Morada = clienteAtualizado.Morada;
            cliente.Ativo = clienteAtualizado.Ativo;

            try
            {
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                return  ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, cliente));
        }

        // DELETE api/clientes/5
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var cliente = db.Clientes.SingleOrDefault(e => e.Id == id);

            if (cliente == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Cliente não encontrado"));
            }

            var existeConsumos = db.Consumos.Any(c => c.Contador.ClienteId == id);

            var existeFaturas = db.Faturas.Any(f => f.ClienteId == id);

            if (existeConsumos || existeFaturas)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, 
                    "O cliente não pode ser apagado porque existem consumos ou faturas associadas"));
            }

            try
            {
                db.Clientes.DeleteOnSubmit(cliente);
                db.SubmitChanges();
            }
            catch(Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }
    }
}