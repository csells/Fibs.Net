import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch, Redirect, withRouter } from 'react-router-dom'
import { Navbar, Nav, NavItem } from 'react-bootstrap'
import { LinkContainer } from 'react-router-bootstrap'
import './App.css';

const LoggedInLink = props => props.user ? props.children : null;
const LoggedOutLink = props => props.user ? null : props.children;

const LoggedInRoute = ({ component: Component, ...rest }) => (
  <Route {...rest} render={props => (rest["user"]
      ? <Component {...props} />
      : <Redirect to={{ pathname: '/login', state: { from: props.location } }} />
  )} />
)

// Is this needed? Ever time the user enters a URL manually, they're going to refresh the page anyway, losing the state...
// EXCEPT: How will this interact with saving the user's login info between sessions?
const LoggedOutRoute = ({ component: Component, ...rest }) => (
  <Route {...rest} render={props => (rest["user"]
    ? <Redirect to={{ pathname: '/logout', state: { from: props.location } }} />
    : <Component {...props} />
  )} />
)

const Login = withRouter(props => <button onClick={() => props.login("bob", "bob1", () => {
    const {from} = props.location.state || {from: "/" };
    props.history.push(from);
  })}>Login</button>);
const Logout = withRouter(props => <button onClick={() => props.logout(() => props.history.push("/"))}>Logout</button>);
const Play = () => <div>Play!</div>;

class App extends Component {
  state = { user: "", pw: "" };

  login = (user, pw, cb) => { this.setState({ user: user, pw: pw }); cb(); }
  logout = (cb) => { this.setState({ user: "", pw: "" }); cb(); }

  render() {
    const user = this.state.user;
    console.log(`user: ${user}`);

    return (
      <Router>
        <div>
          <Navbar collapseOnSelect>
            <Navbar.Header>
              <Navbar.Brand>Play FIBS!</Navbar.Brand>
              <Navbar.Toggle />
            </Navbar.Header>
            <Navbar.Collapse>
              <Nav>
                <LinkContainer exact to="/"><NavItem>Welcome</NavItem></LinkContainer>
                <LoggedOutLink user={user}><LinkContainer exact to="/login"><NavItem>Login</NavItem></LinkContainer></LoggedOutLink>
                <LoggedInLink user={user}><LinkContainer exact to="/logout"><NavItem>Logout</NavItem></LinkContainer></LoggedInLink>
                <LoggedInLink user={user}><LinkContainer exact to="/play"><NavItem>Play</NavItem></LinkContainer></LoggedInLink>
              </Nav>
            </Navbar.Collapse>
          </Navbar>
          <Switch>
            <Route exact path="/" render={() => <div>Welcome!</div>} />
            <LoggedOutRoute user={user} exact path="/login" component={() => <Login login={this.login} />} />
            <LoggedInRoute user={user} exact path="/logout" component={() => <Logout logout={this.logout} />} />
            <LoggedInRoute user={user} exact path="/play" component={() => <Play />} />
            <Route render={() => <div>404</div>} />
          </Switch>
          <div>user: {user}</div>
        </div>
      </Router>
    );
  }
}

export default App;
