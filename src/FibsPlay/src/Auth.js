import React from 'react';
import { Route, Redirect, withRouter } from 'react-router-dom'
import { LinkContainer } from 'react-router-bootstrap'

export function LoggedInLinkContainer({ user, ...rest }) { return user ? <LinkContainer {...rest} /> : null; }
export function LoggedOutLinkContainer({ user, ...rest }) { return user ? null : <LinkContainer {...rest} />; }

export function LoggedInRoute({ component: Component, user, ...rest }) {
  return (
    <Route {...rest} render={props => (user
      ? <Component {...props} />
      : <Redirect to={{ pathname: '/login', state: { from: props.location } }} />
    )} />
  );
}

const Login = withRouter(props => <button onClick={() => props.login("bob", "bob1", () => {
  const { from } = props.location.state || { from: "/" };
  props.history.push(from);
})}>Login</button>);
export { Login };

const Register = withRouter(props => <button onClick={() => props.register("bob", "bob1", () => {
  const { from } = props.location.state || { from: "/" };
  props.history.push(from);
})}>Register</button>);
export { Register };

const Logout = withRouter(props => <button onClick={() => props.logout(() => props.history.push("/"))}>Logout</button>);
export { Logout };
