﻿using CleanArchitectureExample.Domain.Core.DomainNotification;
using CleanArchitectureExample.Domain.Entities;
using CleanArchitectureExample.Domain.Enums;
using CleanArchitectureExample.Domain.Interfaces.Persistence.Repositories;
using CleanArchitectureExample.Domain.RequestHandlers.BookLoanHandlers.Commands.ReturnBook;
using CleanArchitectureExample.Domain.Resources;
using CleanArchitectureExample.Tests.Base;
using CleanArchitectureExample.Tests.Factories;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CleanArchitectureExample.Tests.BookLoanTests.Commands
{
    public class ReturnBookCommandTests: TestBaseArrangements
    {
        public ReturnBookCommandTests():base()
        {
        }

        
        [InlineData(" ", "Loan Guid is empty")]
        [InlineData("00000000-0000-0000-0000-000000000000", "Loan Guid string is empty")]
        [InlineData("3213-12312-1232", "Loan Guid is invalid")]
        public async void HandleReturnBookCommand_WithInvalidGuid_ShouldReturnInvalidLoanGuid(string loanId, string outputError)
        {
            //Arrange
            ReturnBookCommand returnBookCommand = new ReturnBookCommand(loanId);
            var sut = Mocker.CreateInstance<ReturnBookCommandHandler>();

            //Act
            ReturnBookCommandResponseViewModel result = await sut.Handle(returnBookCommand, new CancellationToken());

            //Assert

            DomainNotifications.GetAll().Should().NotBeNullOrEmpty()
                                                          .And.Contain(x => x == outputError);
            Mocker.GetMock<IBookLoanRepository>().Verify(x => x.Update(It.IsAny<BookLoan>()), Times.Never());

        }

        [Fact]
        public async void HandleReturnBookCommand_WithInvalidLoan_ShouldReturnInvalidLoan() 
        {
            //Arrange
            ReturnBookCommand returnBookCommand = new ReturnBookCommand(Guid.NewGuid().ToString());
            var sut = Mocker.CreateInstance<ReturnBookCommandHandler>();

            Mocker.GetMock<IBookLoanRepository>()
                                   .Setup(p => p.GetByLoanId(It.IsAny<Guid>(), It.IsAny<bool>()))
                                   .Returns(() => Task.FromResult<BookLoan>(null));

            //Act
            ReturnBookCommandResponseViewModel result = await sut.Handle(returnBookCommand, new CancellationToken());

            //Assert
            DomainNotifications.GetAll().Should().NotBeNullOrEmpty()
                                                         .And.Contain(x => x == "Book loan not found");

            Mocker.GetMock<IBookLoanRepository>().Verify(x => x.Update(It.IsAny<BookLoan>()), Times.Never());

        }

        [Fact]
        public async void HandleReturnBookCommand_WithValidInfos_ShouldReturnNoErrors()
        {
            //Arrange
            ReturnBookCommand returnBookCommand = new ReturnBookCommand(Guid.NewGuid().ToString());
            var sut = Mocker.CreateInstance<ReturnBookCommandHandler>();

            BookLoan bookLoan = BookLoanFactory.ReturnLoan();
            bookLoan.Book.WithBookSituation(BookSituationEnum.Lent);


            Mocker.GetMock<IBookLoanRepository>()
                       .Setup(p => p.GetByLoanId(It.IsAny<Guid>(), It.IsAny<bool>()))
                       .Returns(() => Task.FromResult<BookLoan>(bookLoan));

            //Act
            ReturnBookCommandResponseViewModel result = await sut.Handle(returnBookCommand, new CancellationToken());

            //Assert
            DomainNotifications.GetAll().Should().BeEmpty();

            Mocker.GetMock<IBookLoanRepository>().Verify(x => x.Update(It.IsAny<BookLoan>()), Times.Once());

        }
    }
}
