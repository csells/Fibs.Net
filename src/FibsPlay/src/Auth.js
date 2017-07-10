import React from 'react';
import { Route, Redirect, withRouter } from 'react-router-dom'
import { LinkContainer } from 'react-router-bootstrap'

const LoggedInLinkContainer = ({ user, ...rest }) => user ? <LinkContainer {...rest} /> : null;
const LoggedOutLinkContainer = ({ user, ...rest }) => user ? null : <LinkContainer {...rest} />;

const LoggedInRoute = ({ component: Component, user, ...rest }) => (
  <Route {...rest} render={props => (user
    ? <Component {...props} />
    : <Redirect to={{ pathname: '/login', state: { from: props.location } }} />
  )} />
)

// Is this needed? Ever time the user enters a URL manually, they're going to refresh the page anyway, losing the state...
// EXCEPT: How will this interact with saving the user's login info between sessions?
const LoggedOutRoute = ({ component: Component, user, ...rest }) => (
  <Route {...rest} render={props => (user
    ? <Redirect to={{ pathname: '/logout', state: { from: props.location } }} />
    : <Component {...props} />
  )} />
)

const Login = withRouter(props => <button onClick={() => props.login("bob", "bob1", () => {
  const { from } = props.location.state || { from: "/" };
  props.history.push(from);
})}>Login</button>);

const Register = withRouter(props => <button onClick={() => props.login("bob", "bob1", () => {
  const { from } = props.location.state || { from: "/" };
  props.history.push(from);
})}>Register</button>);

const Logout = withRouter(props => <button onClick={() => props.logout(() => props.history.push("/"))}>Logout</button>);

export { LoggedInLinkContainer, LoggedOutLinkContainer, LoggedInRoute, LoggedOutRoute, Login, Register, Logout };