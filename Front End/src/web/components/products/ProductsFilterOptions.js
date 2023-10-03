import React, { Component } from 'react';
import {Link} from 'react-router-dom'

class ProductsFilterOptions extends Component {


    constructor(props){
        super(props);       
    }


    handleGrid = (evt, e) => {
        this.props.onClick(e);
        let i, aLinks;

        aLinks = document.getElementsByTagName("a");
        for (i = 0; i < aLinks.length; i++) {
            aLinks[i].className = aLinks[i].className.replace("active", "");
        }

        evt.currentTarget.className += " active";
    }

    render() {
        return (
            <div className="products-filter-options">
                <div className="row align-items-center">
                    <div className="col d-flex">
                        <span>View:</span>

                        <div className="view-list-row">
                            <div className="view-column">
                                <Link to="#"
                                 className="icon-view-two"
                                 onClick={e => {
                                     e.preventDefault();
                                     this.handleGrid(e, "products-col-two")
                                 }}>
                                    
                                        <span></span>
                                        <span></span>
                                   
                                </Link>

                                <Link to="#"
                                 className="icon-view-three active"
                                 onClick={e => {
                                     e.preventDefault();
                                     this.handleGrid(e, "")
                                 }}>
                                   
                                        <span></span>
                                        <span></span>
                                        <span></span>
                                   
                                </Link>

                                <Link to="#"
                                 className="icon-view-four"
                                 onClick={e => {
                                     e.preventDefault();
                                     this.handleGrid(e, "products-col-four")
                                 }}>
                                  
                                        <span></span>
                                        <span></span>
                                        <span></span>
                                        <span></span>
                                 
                                </Link>

                                <Link to="#"
                                className="view-grid-switch"
                                onClick={e => {
                                    e.preventDefault();
                                    this.handleGrid(e, "products-row-view")
                                }}>
                                   
                                        <span></span>
                                        <span></span>
                                  
                                </Link>
                            </div>
                        </div>
                    </div>

                    <div className="col d-flex justify-content-center">
                        <p>Showing {this.props.PageNo} to {this.props.PageSize} of {this.props.TotalRecords} results</p>
                    </div>

                    <div className="col d-flex">
                        <span>Show:</span>

                        <div className="show-products-number">
                            <select onChange={(e) => this.props.setPageSizeFromProductFilter(e)}>
                                <option value="22">9</option>
                                <option value="22">18</option>
                                <option value="32">27</option>
                                <option value="42">36</option>
                                <option value="52">45</option>
                                <option value="62">54</option>
                                <option value="100">100</option>
                            </select>
                        </div>

                        <span>Sort:</span>

                        <div className="products-ordering-list">
                        <select onChange={(e) => this.props.setSortByFilter(e)}>
                                <option value="">Featured</option>
                             
                                <option value="Price ASC">Price Ascending</option>
                                <option value="Price DESC">Price Descending</option>
                                <option value="Date DESC">Date Ascending</option>
                                <option value="Date ASC">Date Descending</option>
                               
                            </select>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

export default ProductsFilterOptions;
