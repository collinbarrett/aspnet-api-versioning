namespace Microsoft.Web.Http.Versioning.Conventions
{
    using FluentAssertions;
    using Moq;
    using System;
    using System.Linq;
    using System.Web.Http;
    using Xunit;
    using static Moq.Times;

    public class ActionConventionBuilderExtensionsTest
    {
        [Fact]
        public void action_should_map_method_from_action_delegate_expression()
        {
            // arrange
            var method = typeof( StubController ).GetMethod( nameof( StubController.Delete ) );
            var controllerBuilder = new ControllerApiVersionConventionBuilder<StubController>();
            var actionBuilder = new Mock<ActionApiVersionConventionBuilder<StubController>>( controllerBuilder );

            // act
            actionBuilder.Object.Action( c => c.Delete() );

            // assert
            actionBuilder.Verify( b => b.Action( method ), Once() );
        }

        [Fact]
        public void action_should_map_method_from_func_delegate_expression()
        {
            // arrange
            var method = typeof( StubController ).GetMethod( nameof( StubController.Get ) );
            var controllerBuilder = new ControllerApiVersionConventionBuilder<StubController>();
            var actionBuilder = new Mock<ActionApiVersionConventionBuilder<StubController>>( controllerBuilder );

            // act
            actionBuilder.Object.Action( c => c.Get() );

            // assert
            actionBuilder.Verify( b => b.Action( method ), Once() );
        }

        [Fact]
        public void action_should_throw_exception_when_func_delegate_expression_is_not_a_method()
        {
            // arrange
            var controllerBuilder = new ControllerApiVersionConventionBuilder<StubController>();
            var actionBuilder = new Mock<ActionApiVersionConventionBuilder<StubController>>( controllerBuilder ).Object;

            // act
            Action action = () => actionBuilder.Action( c => c.Timeout );

            // assert
            action.Should().Throw<InvalidOperationException>().And
                  .Message.Should().Be( "The expression 'c => c.Timeout' must refer to a controller action method." );
        }

        [Fact]
        public void action_should_map_method_from_name()
        {
            // arrange
            const string methodName = nameof( StubController.Post );
            var controllerType = typeof( StubController );
            var method = controllerType.GetMethods().Single( m => m.Name == methodName && m.GetParameters().Length == 0 );
            var builder = new Mock<ActionApiVersionConventionBuilder>( new ControllerApiVersionConventionBuilder( controllerType ) );

            // act
            builder.Object.Action( methodName );

            // assert
            builder.Verify( b => b.Action( method ), Once() );
        }

        [Fact]
        public void action_should_map_method_from_name_and_argument_type()
        {
            // arrange
            const string methodName = nameof( StubController.Post );
            var controllerType = typeof( StubController );
            var method = controllerType.GetMethods().Single( m => m.Name == methodName && m.GetParameters().Length == 1 );
            var builder = new Mock<ActionApiVersionConventionBuilder>( new ControllerApiVersionConventionBuilder( controllerType ) );

            // act
            builder.Object.Action( methodName, typeof( int ) );

            // assert
            builder.Verify( b => b.Action( method ), Once() );
        }

        [Fact]
        public void action_should_throw_exception_when_method_does_not_exist()
        {
            // arrange
            var message = "An action method with the name 'NoSuchMethod' could not be found. The method must be public, non-static, and not have the NonActionAttribute applied.";
            var builder = new Mock<ActionApiVersionConventionBuilder>( new ControllerApiVersionConventionBuilder( typeof( StubController ) ) );

            // act
            Action actionConvention = () => builder.Object.Action( "NoSuchMethod" );

            // assert
            actionConvention.Should().Throw<MissingMethodException>().And.Message.Should().Be( message );
        }

        public sealed class StubController : ApiController
        {
            public IHttpActionResult Get() => Ok();

            public void Delete() { }

            public TimeSpan Timeout { get; set; }

            public IHttpActionResult Post() => Post( 42, "stubs/42" );

            public IHttpActionResult Post( int id ) => Ok();

            [NonAction]
            public IHttpActionResult Post( int id, string location ) => Created( location, new { id = id } );
        }
    }
}