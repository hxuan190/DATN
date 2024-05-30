﻿using asd123.Helpers;
using asd123.Services;
using asd123.Ultil;
using Microsoft.EntityFrameworkCore;
using System;

namespace asd123.UseCase.Subject.Crud
{
    public class CrudSubjectFlow
    {
        private readonly IUnitOfWork unitOfWork;

        public CrudSubjectFlow(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public ResponseData List()
        {
            try
            {
                var subjects = unitOfWork.Subjects.FindAll();
                return new ResponseData(Message.SUCCESS, subjects);
            }
            catch (Exception ex)
            {
                return new ResponseData(Message.ERROR, $"An error occurred: {ex.Message}");
            }
        }

        public ResponseData FindByName(string name)
        {
            try
            {
                var existingMajor = unitOfWork.Majors.GetCodeMajor(name);
                if (existingMajor == null)
                {
                    return new ResponseData(Message.ERROR, "Major not found");
                }
                return new ResponseData(Message.SUCCESS, existingMajor);
            }
            catch (Exception ex)
            {
                return new ResponseData(Message.ERROR, $"An error occurred: {ex.Message}");
            }
        }

        public ResponseData Create(asd123.Model.Subject subject)
        {
            try
            {
                var result = unitOfWork.Subjects.Create(subject);
                return new ResponseData(Message.SUCCESS, result);
            }
            catch (Exception ex)
            {
                return new ResponseData(Message.ERROR, $"An error occurred: {ex.Message}");
            }
        }

        public ResponseData Update(asd123.Model.Subject subject, string code)
        {
            try
            {
                var existingSubject = unitOfWork.Subjects.GetCodeSubject(code);
                if (existingSubject == null)
                {
                    return new ResponseData(Message.ERROR, "Subject not found");
                }

                existingSubject.Code = subject.Code;
                existingSubject.Name = subject.Name;
                existingSubject.TotalCreadits = subject.TotalCreadits;
                existingSubject.UpdatedAt = subject.UpdatedAt;
                unitOfWork.SaveChanges();

                return new ResponseData(Message.SUCCESS, existingSubject);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ResponseData(Message.ERROR, "The entity being updated has been modified by another user. Please reload the entity and try again.");
            }
            catch (Exception ex)
            {
                return new ResponseData(Message.ERROR, $"An error occurred: {ex.Message}");
            }
        }

        public ResponseData Delete(string code)
        {
            try
            {
                var existingSubject = unitOfWork.Subjects.GetCodeSubject(code);
                if (existingSubject == null)
                {
                    return new ResponseData(Message.ERROR, "Subject not found");
                }

                var result = unitOfWork.Subjects.Delete(existingSubject.Id);
                return new ResponseData(Message.SUCCESS, result);
            }
            catch (Exception ex)
            {
                return new ResponseData(Message.ERROR, $"An error occurred: {ex.Message}");
            }
        }
    }
}
