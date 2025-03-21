import React from 'react';
import { Link } from 'react-router-dom';

const Navbar = () => {
  return (
    <nav className="navbar">
      <ul className="navbar-links">
        <li>
          <Link to="/auth/login">Login</Link>
        </li>
        <li>
          <Link to="/auth/register">Register</Link>
        </li>
      </ul>
    </nav>
  );
};

export default Navbar;