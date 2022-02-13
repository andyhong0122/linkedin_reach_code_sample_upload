using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sabio.Models;
using Sabio.Models.Domain;
using Sabio.Models.Requests;
using Sabio.Services;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;

namespace Sabio.Web.Api.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FileApiController : BaseApiController
    {
        private IFileService _service = null;
        private IAuthenticationService<int> _authService = null;

        public FileApiController(
            IFileService service
            , ILogger<FileApiController> logger
            , IAuthenticationService<int> authService) : base(logger)
        {
            _service = service;
            _authService = authService;
        }        
        

        [HttpPost]
        public async Task<ActionResult<ItemsResponse<string>>>Upload(List<IFormFile> files)
        {
            int code = 201;
            BaseResponse response = null;
            try
            {
                if (files == null)
                {
                    code = 400;
                    response = new ErrorResponse("File invalid or not found");
                }
                else if (files != null && files.Count > 0)
                {
                    // send list of files to upload the uploader
                    List<File> uploadedFiles = await _service.Upload(files, _authService.GetCurrentUserId());
                    response = new ItemsResponse<File> { Items = uploadedFiles };
                }
            }
            catch(Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"ArgumentException Error:${ex.Message}");
            }
            return StatusCode(code, response);
        }


        [HttpGet]
        public ActionResult<ItemResponse<Paged<File>>> GetPaginate(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Paged<File> page = _service.GetPaginate(pageIndex, pageSize);

                if (page == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<File>> { Item = page };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }


        [HttpGet("{id:int}")]
        public ActionResult<ItemsResponse<File>> GetById(int id)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                File file = _service.GetById(id);

                if (file == null)
                {
                    iCode = 404;
                    response = new ErrorResponse("Resource not found");
                }
                else
                {
                    response = new ItemResponse<File> { Item = file };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"ArgumentException Error:${ex.Message}");
            }
            return StatusCode(iCode, response);
        }


        [HttpGet("current")]
        public ActionResult<ItemResponse<Paged<File>>> GetByCreatedBy(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                int userId = _authService.GetCurrentUserId();

                Paged<File> page = _service.GetByCreatedBy(pageIndex, pageSize, userId);

                if (page == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<File>> { Item = page };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }


        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> Update(FileUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                int userId = _authService.GetCurrentUserId();
                _service.Update(model, userId);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }



        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                response = new SuccessResponse();
                _service.Delete(id);
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }
    }

}