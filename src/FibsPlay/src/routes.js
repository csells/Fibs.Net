import React from 'react';
import { BrowserRouter, Route, Link, Switch } from 'react-router-dom'
import { Navbar, Nav, NavItem } from 'react-bootstrap'
import App from './App';
import About from './About';
import NotFound from './NotFound';

// TODO: add auth flow from https://reacttraining.com/react-router/web/example/auth-workflow
const Routes = (props) => (
  <BrowserRouter>
    <div>
      <Navbar collapseOnSelect="true">
        <Navbar.Header>
          <Navbar.Brand>
            Welcome to React App!
          </Navbar.Brand>
          <Navbar.Toggle />
        </Navbar.Header>
        <Navbar.Collapse>
          <Nav>
            <NavItem><Link to="/">Home</Link></NavItem>
            <NavItem><Link to="/about">About</Link></NavItem>
          </Nav>
        </Navbar.Collapse>
      </Navbar>
      <Switch>
        <Route exact path="/" component={App} />
        <Route path="/about" component={About} />
        <Route component={NotFound} />
      </Switch>
    </div>
  </BrowserRouter>
);

export default Routes;