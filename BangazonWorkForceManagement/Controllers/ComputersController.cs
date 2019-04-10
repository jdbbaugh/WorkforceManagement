﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using BangazonWorkForceManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonWorkForceManagement.Controllers
{
    public class ComputersController : Controller
    {
        private readonly IConfiguration _configuration;

        public ComputersController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: Computers
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id,
                                               PurchaseDate, DecomissionDate, 
                                               Make, Manufacturer
                                          FROM Computer";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> computers = new List<Computer>();

                    while (reader.Read())
                    {
                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))
                        };

                        computers.Add(computer);
                    }

                    reader.Close();
                    return View(computers);
                }
            }
        }

        // GET: Computers/Details/5
        public ActionResult Details(int id)
        {
            Computer computer = GetComputerById(id);
            if (computer == null)
            {
                return NotFound();
            }

            ComputerDetailViewModel viewModel = new ComputerDetailViewModel
            {
                Id = id,
                Make = computer.Make,
                Manufacturer = computer.Manufacturer,
                PurchaseDate = computer.PurchaseDate,
                DecommissionDate = computer.DecommissionDate
            };

            return View(viewModel);
        }

        // GET: Computers/Create
        public ActionResult Create()
        {
            ComputerCreateViewModel viewModel =
                new ComputerCreateViewModel();
            return View(viewModel);
        }

        // POST: Computers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ComputerCreateViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO computer (
                                               PurchaseDate,  
                                               Make, Manufacturer)
                                             VALUES (@PurchaseDate, 
                                                @Make, @Manufacturer)";
                        cmd.Parameters.Add(new SqlParameter("@PurchaseDate", viewModel.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@Make", viewModel.Make));
                        cmd.Parameters.Add(new SqlParameter("@Manufacturer", viewModel.Manufacturer));
                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Computers/Delete/5
        public ActionResult Delete(int id)
        {
            Computer computer = GetComputerById(id);
            if (computer == null)
            {
                return NotFound();
            }
            ComputerDeleteViewModel viewModel = new ComputerDeleteViewModel
            {
                Id = id,
                Make = computer.Make,
                Manufacturer = computer.Manufacturer,
                PurchaseDate = computer.PurchaseDate
            };

            return View(viewModel);
        }

        public ActionResult DeleteFailure(int id)
        {
            Computer computer = GetComputerById(id);
            if (computer == null)
            {
                return NotFound();
            }

            ComputerDeleteFailureViewModel viewModel = new ComputerDeleteFailureViewModel
            {
                Id = id,
                Make = computer.Make,
                Manufacturer = computer.Manufacturer,
                PurchaseDate = computer.PurchaseDate
            };

            return View(viewModel);
        }

        // POST: Computers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM computer WHERE id = @id;";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return Redirect(DeleteFailure(id).ToString());
            }
        }

        

        private Computer GetComputerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id,
                                               PurchaseDate, DecomissionDate, 
                                               Make, Manufacturer
                                          FROM Computer
                                            WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computer = null;

                    if (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))
                        };
                    }

                    reader.Close();

                    return computer;
                }
            }

        }
    }
}