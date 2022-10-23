DROP TABLE IF EXISTS user_inventory;
CREATE TABLE user_inventory (
    SetCode varchar(5), 
    CollectorNumber varchar(3), 
    Name varchar(128), 
    Attrs varchar(25), 
    Count int 
);
CREATE UNIQUE INDEX idx_CollectorNumber_Attrs ON user_inventory (CollectorNumber, Attrs);