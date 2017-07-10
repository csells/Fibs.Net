import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom'
import { Navbar, Nav, NavItem } from 'react-bootstrap'
import { LinkContainer } from 'react-router-bootstrap'
import { LoggedInLinkContainer, LoggedOutLinkContainer, LoggedInRoute, LoggedOutRoute, Login, Register, Logout } from "./Auth";
import './App.css';
import Welcome from './Welcome';
import Watch from './Watch';

export default class App extends Component {
  state = { user: "", pw: "" };

  login = (user, pw, cb) => { this.setState({ user: user, pw: pw }); cb(); } // TODO: log into FIBS
  register = (user, pw, cb) => { this.setState({ user: user, pw: pw }); cb(); } // TODO: register a new user with FIBS
  logout = (cb) => { this.setState({ user: "", pw: "" }); cb(); } //  TODO: log out of FIBS

  render() {
    const user = this.state.user;

    return (
      <Router>
        <div>
          <Navbar collapseOnSelect fixedTop>
            <Navbar.Header>
              <Navbar.Brand>Play FIBS!</Navbar.Brand>
              <Navbar.Toggle />
            </Navbar.Header>
            <Navbar.Collapse>
              <Nav>
                <LinkContainer exact to="/"><NavItem>Welcome</NavItem></LinkContainer>
                <LoggedOutLinkContainer user={user} exact to="/login"><NavItem>Login</NavItem></LoggedOutLinkContainer>
                <LoggedOutLinkContainer user={user} exact to="/register"><NavItem>Register</NavItem></LoggedOutLinkContainer>
                <LoggedInLinkContainer user={user} exact to="/logout"><NavItem>Logout</NavItem></LoggedInLinkContainer>
                <LoggedInLinkContainer user={user} exact to="/watch"><NavItem>Watch</NavItem></LoggedInLinkContainer>
              </Nav>
            </Navbar.Collapse>
          </Navbar>
          <Switch>
            <Route exact path="/" component={Welcome} />
            <LoggedOutRoute user={user} exact path="/login" component={() => <Login login={this.login} />} />
            <LoggedOutRoute user={user} exact path="/register" component={() => <Register register={this.register} />} />
            <LoggedInRoute user={user} exact path="/logout" component={() => <Logout logout={this.logout} />} />
            <LoggedInRoute user={user} exact path="/watch" component={() => <Watch />} />
            <Route render={() => <div>404</div>} />
          </Switch>
          <div>user: {user}</div>
        </div>
      </Router>
    );
  }
}
