import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { MakeApiCallSynchronous , MakeApiCallAsync } from '../../../helpers/ApiHelpers';
    
import Config from '../../../helpers/Config';


const SizeGuide = (props) => {

    const [SizeList, setSizeList] = useState([]);


    useEffect(() => {
        // declare the data fetching function
        const getSizeList = async () => {

             
            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
                  
            }


            const param = {
                requestParameters: {
                    PageNo: 1,
                    PageSize: 100,
                    recordValueJson: "[]",
                },
            };
      
            //--Get sizes list
            const sizeResponse = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_SIZE_LIST'], null, param, headers, "POST", true);
            if (sizeResponse != null && sizeResponse.data != null) {
                setSizeList(JSON.parse(sizeResponse.data.data));

                console.log(JSON.parse(sizeResponse.data.data));
            }



        }

        // call the function
        getSizeList().catch(console.error);
    }, [])


    return (
        <>
            <div
                className="modal fade sizeGuideModal show" style={{ paddingRight: '16px', display: 'block' }}
            >
                <div className="modal-dialog modal-dialog-centered" role="document">
                    <div className="modal-content">
                        <button type="button"
                            onClick={(e) => {
                                e.preventDefault();
                                props.closeSizeGuide();

                            }}



                            className="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">
                                <i className="fas fa-times"></i>
                            </span>
                        </button>

                        <div className="modal-sizeguide">
                            <h3>Size Guide</h3>
                            <p>This is an approximate conversion table to help you find your size.</p>

                            <div className="table-responsive">
                                <table className="table table-striped">
                                    <thead>
                                        <tr>
                                            <th>Name</th>
                                            <th>Short Name</th>
                                            <th>Inches</th>
                                            <th>Centimeters</th>

                                        </tr>
                                    </thead>

                                    <tbody>

                                        {
                                            SizeList?.map((item, idx) =>

                                                <tr>
                                                    <td>{item.Name}</td>
                                                    <td>{item.ShortName}</td>
                                                    <td>{item.Inches}</td>
                                                    <td>{item.Centimeters}</td>
                                                   
                                                </tr>
                                            )}




                                      
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}

export default SizeGuide;