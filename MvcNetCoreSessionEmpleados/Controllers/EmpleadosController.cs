using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MvcNetCoreSessionEmpleados.Extensions;
using MvcNetCoreSessionEmpleados.Models;
using MvcNetCoreSessionEmpleados.Repositories;

namespace MvcNetCoreSessionEmpleados.Controllers
{
    public class EmpleadosController : Controller
    {
        private RepositoryEmpleados repo;
        private IMemoryCache memoryCache;

        public EmpleadosController(RepositoryEmpleados repo, IMemoryCache memoryCache)
        {
            this.repo = repo;
            this.memoryCache = memoryCache;
        }

        public async Task<IActionResult> SessionSalarios(int? salario)
        {
            if(salario != null)
            {
                //NECESITAMOS ALMACENAR EL SALARIO DEL EMPLEADO 
                //Y LA SUMA TOTAL DE SALARIOS QUE TENGAMOS
                int sumaSalarial = 0;

                //PREGUNTAREMOS SI YA TENEMOS LA SUMA ALMACENADA EN SESSION
                if(HttpContext.Session.GetString("SUMASALARIAL") != null)
                {
                    //SI YA EXISTE LA SUMA SALARIAL, RECUPERAMOS SU VALOR
                    sumaSalarial = HttpContext.Session.GetObject<int>("SUMASALARIAL");
                }
                //SI NO EXISTE, REALIZAMOS LA SUMA
                sumaSalarial += salario.Value;

                //ALMACENAMOS EL NUEVO VALOR DE LA SUMA SALARIAL DENTRO DE SESSION
                HttpContext.Session.SetObject("SUMASALARIAL", sumaSalarial);

                ViewData["MENSAJE"] = "Salario almacenado: " + salario.Value;
            }

            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public IActionResult SumaSalarial()
        {
            return View();
        }

        public async Task<IActionResult> SessionEmpleados(int? idEmpleado)
        {
            if(idEmpleado != null)
            {
                //BUSCAMOS AL EMPLEADO
                Empleado empleado = await this.repo.FindEmpleadoAsync(idEmpleado.Value);

                //EN SESSION TENDREMOS UN CONJUNTO DE EMPLEADOS
                List<Empleado> empleadosList;

                //DEBEMOS PREGUNTAR SI TENEMOS EL CONJUNTO DE EMPLEADOS 
                //ALMACENADOS EN SESSION
                if(HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS") != null)
                {
                    //RECUPERAMOS LOS EMPLEADOS QUE TENGAMOS EN SESSION
                    empleadosList = HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS");
                }
                else
                {
                    //SI NO EXISTE, INSTANCIAMOS LA COLECCION
                    empleadosList = new List<Empleado>();
                }
                //ALMACENAMOS EL EMPLEADO DENTRO DE NUESTRA COLECCION
                empleadosList.Add(empleado);
                //GUARDAMOS EN SESSION LA COLECCION CON EL NUEVO EMPLEADO
                HttpContext.Session.SetObject("EMPLEADOS", empleadosList);
                
                ViewData["MENSAJE"] = "Empleado " + empleado.Apellido + " almacenado correctamente.";
            }
            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public IActionResult EmpleadosAlmacenados() 
        {
            return View();
        }

        public async Task<IActionResult> SessionEmpleadosOk(int? idEmpleado, int? idfavorito)
        {
            if(idfavorito != null)
            {
                //COMO ESTOY ALMACENANDO EN CACHE DE CLIENTE, VAMOS A UTILIZAR LOS OBJETOS EN LUGAR DE LOS IDS.
                List<Empleado> empleadosFavoritos;

                if(this.memoryCache.Get("FAVORITOS") == null)
                {
                    //NO EXISTE, CREAMOS LA COLECCION DE FAVORITOS
                    empleadosFavoritos = new List<Empleado>();
                }
                else
                {
                    //EXISTE,RECUPERAMOS LOS EMPLEADOS QUE TENEMOS EN LA COLECCION DE FAVORITOS DE CACHE.
                    empleadosFavoritos = this.memoryCache.Get<List<Empleado>>("FAVORITOS");
                }

                //BUSCAMOS EL ONJETO EMPLEADO A ALMACENAR
                Empleado emp = await this.repo.FindEmpleadoAsync(idfavorito.Value);
                empleadosFavoritos.Add(emp);
                this.memoryCache.Set("FAVORITOS", empleadosFavoritos);
            }

            if(idEmpleado != null)
            {
                //ALMACENAREMOS LO MINIMO QUE PODAMOS (int)
                List<int> idsEmpleados;

                //DEBEMOS PREGUNTAR SI TENEMOS EL CONJUNTO DE IDSEMPLEADOS 
                //ALMACENADOS EN SESSION
                if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS") == null)
                {
                    //NO EXISTE Y CREAMOS LA COLECCION
                    idsEmpleados = new List<int>();
                }
                else
                {
                    //EXISTE Y RECUPERAMOS LA COLECCION
                    idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
                }
                idsEmpleados.Add(idEmpleado.Value);
                //REFRESCAMOS LOS DATOS DE SESSION
                HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);

                ViewData["MENSAJE"] = "Empleados almacenados: " + idsEmpleados.Count();
            }
            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public async Task<IActionResult> EmpleadosAlmacenadosOk()
        {
            //DEBEMOS RECUPERAR LOS IDS DE EMPLEADOS QUE TENGAMOS EN SESSION
            List<int> idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");

            if(idsEmpleados == null)
            {
                ViewData["MENSAJE"] = "No existen empleados almacenados en Session";
                return View();
            }
            else
            {
                List<Empleado> empleados = await this.repo.GetEmpleadosSessionAsync(idsEmpleados);
                return View(empleados);
            }
        }

        public async Task<IActionResult> SessionEmpleadosV4(int? idEmpleado)
        {
            if (idEmpleado != null)
            {
                List<int> idsEmpleados;

                if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS") == null)
                {
                    idsEmpleados = new List<int>();
                }
                else
                {
                    idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
                }
                idsEmpleados.Add(idEmpleado.Value);
                HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                ViewData["MENSAJE"] = "Empleados almacenados: " + idsEmpleados.Count;
            }
            //COMPROBAMOS SI TENEMOS IDS EN SESSION
            List<int> ids = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");

            if (ids == null)
            {
                List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
                return View(empleados);
            }
            else
            {
                List<Empleado> empleados =  await this.repo.GetEmpleadosNotSessionAsync(ids);
                return View(empleados);
            }
        }

        public async Task<IActionResult> EmpleadosAlmacenadosV4()
        {
            List<int> idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
            if (idsEmpleados == null)
            {
                ViewData["MENSAJE"] = "No existen empleados almacenados en Session.";
                return View();
            }
            else
            {
                List<Empleado> empleados = await this.repo.GetEmpleadosSessionAsync(idsEmpleados);
                return View(empleados);
            }
        }

        public IActionResult QuitarEmpleadoSession(int idEmpleado)
        {
            // Recuperamos la lista de empleados almacenados en sesión
            List<int> idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");

            if (idsEmpleados != null && idsEmpleados.Contains(idEmpleado))
            {
                // Eliminamos el ID del empleado de la lista
                idsEmpleados.Remove(idEmpleado);

                // Si la lista queda vacía, eliminamos la clave de sesión
                if (idsEmpleados.Count == 0)
                {
                    HttpContext.Session.Remove("IDSEMPLEADOS");
                }
                else
                {
                    // Actualizamos la sesión con la nueva lista de empleados
                    HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                }
            }

            // Redirigir de nuevo a la vista de empleados almacenados
            return RedirectToAction("EmpleadosAlmacenadosOk");
        }

        public IActionResult EmpleadosFavoritos(int? ideliminar)
        {
            //PREGUNTAMOS SI EXISTEN FAVORITOS
            //if(this.memoryCache.Get("FAVORITOS") == null)
            //{
            //    ViewData["MENSAJE"] = "No existen favoritos almacenados.";
            //    return View();
            //}
            //else
            //{
            //    List<Empleado> favoritos = this.memoryCache.Get<List<Empleado>>("FAVORITOS");
            //    return View(favoritos);
            //}

            if(ideliminar != null)
            {
                List<Empleado> favoritos = this.memoryCache.Get<List<Empleado>>("FAVORITOS");

                //BUSCAMOS EL EMPLEADO A ELIMINAR DENTRO DE LA COLECCION DE FAVORITOS
                Empleado empDelete = favoritos.Find(z => z.IdEmpleado == ideliminar.Value);
                //LO ELIMINAMOS DE LA COLECCION
                favoritos.Remove(empDelete);

                if(favoritos.Count == 0)
                {
                    this.memoryCache.Remove("FAVORITOS");
                }
                else
                {
                    this.memoryCache.Set("FAVORITOS", favoritos);
                }
            }

            return View();
        }
    }
}
