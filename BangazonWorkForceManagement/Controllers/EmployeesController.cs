﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using BangazonWorkForceManagement.Models.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonWorkForceManagement.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IConfiguration _config;

        public EmployeesController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: Employees
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.FirstName AS EmployeeFirstName,
                                               e.Id AS EmployeeId,
                                               e.IsSupervisor AS IsSupervisor,
	                                           e.LastName AS EmployeeLastName,
                                               d.Budget AS DepartmentBudget,
	                                           d.Name AS DepartmentName,
                                               d.Id as DepartmentId
                                        FROM Employee e
                                        JOIN Department AS d on d.Id = e.DepartmentId";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("EmployeeFirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("EmployeeLastName")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
                                Budget = reader.GetInt32(reader.GetOrdinal("DepartmentBudget"))
                            }
                        };

                        employees.Add(employee);
                    }
                    reader.Close();
                    return View(employees);
                }
            }
        }

        // GET: Employees/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)

            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.FirstName AS EmployeeFirstName,
                                               e.Id AS EmployeeId,
                                               e.IsSupervisor AS IsSupervisor,
	                                           e.LastName AS EmployeeLastName,
                                               d.Budget AS DepartmentBudget,
	                                           d.Name AS DepartmentName,
                                               d.Id as DepartmentId,
											   c.Make as ComputerMake,
                                               c.Id as ComputerId,
											   c.PurchaseDate as ComputerPurchaseDate,
											   c.DecomissionDate as ComputerDecomissionDate,
											   c.Manufacturer as ComputerManufacturer,
                                               tp.Id as TrainingProgramId,
											   tp.Name as TrainingProgramName,
											   tp.StartDate as TrainingProgramStartDate,
											   tp.EndDate as TrainingProgramEndDate,
											   tp.MaxAttendees as TrainingProgramMaxAtendees
                                        FROM Employee e
                                        JOIN Department AS d on d.Id = e.DepartmentId
										JOIN ComputerEmployee AS ce on ce.EmployeeId = e.Id
										JOIN Computer AS c on c.Id = ce.ComputerId
										JOIN EmployeeTraining AS et on et.EmployeeId = e.Id
										JOIN TrainingProgram AS tp on tp.Id = et.TrainingProgramId
										WHERE e.Id = @id AND ce.UnAssignDate IS NULL";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    EmployeeDetailViewModel employee = null;
                    while (reader.Read())
                    {
                        if (employee == null)
                        {
                            employee = new EmployeeDetailViewModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("EmployeeFirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("EmployeeLastName")),
                                IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Department = new Department
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
                                    Budget = reader.GetInt32(reader.GetOrdinal("DepartmentBudget"))
                                },
                                Computer = new Computer()
                            };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                        {                         
                                employee.Computer = new Computer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                    PurchaseDate = reader.GetDateTime(reader.GetOrdinal("ComputerPurchaseDate")),
                                    DecommissionDate = reader.GetDateTime(reader.GetOrdinal("ComputerDecomissionDate")),
                                    Make = reader.GetString(reader.GetOrdinal("ComputerMake")),
                                    Manufacturer = reader.GetString(reader.GetOrdinal("ComputerManufacturer"))
                                };
                            };
                        
                    
                            if (!reader.IsDBNull(reader.GetOrdinal("TrainingProgramId")))
                            {
                                if (!employee.TrainingProgramList.Exists(x => x.Id == reader.GetInt32(reader.GetOrdinal("TrainingProgramId"))))
                                {
                                    employee.TrainingProgramList.Add(
                                new TrainingProgram
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("TrainingProgramId")),
                                    Name = reader.GetString(reader.GetOrdinal("TrainingProgramName")),
                                    StartDate = reader.GetDateTime(reader.GetOrdinal("TrainingProgramStartDate")),
                                    EndDate = reader.GetDateTime(reader.GetOrdinal("TrainingProgramEndDate")),
                                    MaxAttendees = reader.GetInt32(reader.GetOrdinal("TrainingProgramMaxAtendees"))
                                });
                                }
                            }
                        }                    
                    reader.Close();
                    return View(employee);
                }
            }
        }


        // GET: Employees/Create
        public ActionResult Create()
        {
            {
                EmployeeCreateViewModel viewModel =
                    new EmployeeCreateViewModel(_config.GetConnectionString("DefaultConnection"));
                return View(viewModel);
            }
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeCreateViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"insert into Employee(FirstName, LastName, DepartmentId, IsSuperVisor)
                                             VALUES (@firstname, @lastname, @departmentid, @issupervisor)";

                        cmd.Parameters.Add(new SqlParameter("@firstname", viewModel.Employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastname", viewModel.Employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@departmentid", viewModel.Employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@issupervisor", viewModel.Employee.IsSuperVisor));

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

        // GET: Employees/Edit/5
        public ActionResult Edit(int id)
        {
            Employee employee = GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }

            EmployeeEditViewModel viewModel = new EmployeeEditViewModel
            {
                Departments = GetAllDepartments(),
                Employee = employee
                //TrainingPrograms = GetAllTrainingPrograms()
            };

            return View(viewModel);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeEditViewModel ViewModel)
        {
            try
            {
                // TODO: Add update logic here

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee
                                            SET lastname = @lastname,
                                            DepartmentId = @DepartmentId
                                            WHERE id = @id";                     
                        cmd.Parameters.Add(new SqlParameter("@lastname", ViewModel.Employee.LastName));      
                        cmd.Parameters.Add(new SqlParameter("@DepartmentId", ViewModel.Employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

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



        private Employee GetEmployeeById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS EmployeeId,
                                               e.FirstName, e.LastName, 
                                               e.IsSupervisor, e.DepartmentId
                                         FROM Employee e
                                         WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = null;

                    if (reader.Read())
                    {
                        employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"))             
                        };
                    }

                    reader.Close();

                    return employee;
                }
            }

        }

        private List<Department> GetAllDepartments()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT id, name from Department;";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Department> departments = new List<Department>();

                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("name"))
                        });
                    }
                    reader.Close();

                    return departments;
                }
            }
        }

        private List<TrainingProgram> GetAllTrainingPrograms()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT id, name, startdate, enddate, maxattendees from TrainingProgram;";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        trainingPrograms.Add(new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("startdate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("enddate"))
                        });
                    }
                    reader.Close();

                    return trainingPrograms;
                }
            }
        }
    }
}

