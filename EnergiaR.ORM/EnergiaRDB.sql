-- Base de datos: EnergiaRDB.db
-- Motor:         SQLite: Microsoft.EntityFrameworkCore.Sqlite

-- Tablas
--

-- Historíal que registra operaciones INSERT, UPDATE y DELETE
CREATE TABLE IF NOT EXISTS THistory (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Tabl TEXT COLLATE NOCASE NOT NULL, 
    Oper TEXT COLLATE NOCASE NOT NULL, 
	FHRe DATETIME NOT NULL DEFAULT (DATETIME('now', 'localtime')), 
	RPre TEXT COLLATE NOCASE NULL, 
	RPos TEXT COLLATE NOCASE NULL, 
	CHECK (DATETIME(FHRe) IS NOT NULL)
);

-- Empresas prestadoras de servicio de energía
CREATE TABLE IF NOT EXISTS TEmpre (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
    NDoc TEXT COLLATE NOCASE NOT NULL, 
    RaSo TEXT COLLATE NOCASE NOT NULL, 
    Dire TEXT COLLATE NOCASE NOT NULL, 
    Tele TEXT COLLATE NOCASE NOT NULL, 
    Mail TEXT COLLATE NOCASE NOT NULL, 
    CHECK (LENGTH(NDoc) <= 125), 
    CHECK (LENGTH(RaSo) <= 250), 
    CHECK (LENGTH(Dire) <= 2500), 
    CHECK (LENGTH(Tele) <= 2500), 
    CHECK (LENGTH(Mail) <= 2500)
);

-- Medidores: Casos de uso
CREATE TABLE IF NOT EXISTS TCaUso (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
	CUso TEXT COLLATE NOCASE NOT NULL, 
	CDes TEXT COLLATE NOCASE NOT NULL, 
    CHECK (LENGTH(CUso) <= 20), 
    CHECK (LENGTH(CDes) <= 100)
);

-- Medidores: Clientes/Propietarios
CREATE TABLE IF NOT EXISTS TClien (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
	TDoc TEXT COLLATE NOCASE NOT NULL, 
	NDoc TEXT COLLATE NOCASE NOT NULL, 
	Nomb TEXT COLLATE NOCASE NOT NULL, 
	Sexo INTEGER NOT NULL, 
    CHECK (LENGTH(TDoc) <= 20), 
    CHECK (LENGTH(NDoc) <= 100), 
	CHECK (LENGTH(Nomb) <= 100), 
	CHECK (Sexo IN (0, 1))
);

-- Medidores
CREATE TABLE IF NOT EXISTS TMedid (
	ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
	MNIC TEXT COLLATE NOCASE NOT NULL, 
	Medi TEXT COLLATE NOCASE NOT NULL, 
	Dire TEXT COLLATE NOCASE NOT NULL, 
	CUso INTEGER NOT NULL, 
	Prop INTEGER NOT NULL, 
	CHECK (LENGTH(MNIC) <= 125), 
	CHECK (LENGTH(Medi) <= 125), 
	CHECK (LENGTH(Dire) <= 2500), 
	FOREIGN KEY(CUso) REFERENCES TCaUso(ID) ON DELETE RESTRICT, 
	FOREIGN KEY(Prop) REFERENCES TClien(ID) ON DELETE RESTRICT
);

-- Recargas
CREATE TABLE IF NOT EXISTS TRecar (
	ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Empr INTEGER NOT NULL, 
	FRec DATE NOT NULL DEFAULT (DATE('now', 'localtime')), 
	HRec TIME NOT NULL DEFAULT (TIME('now', 'localtime')), 
	CSeg TEXT COLLATE NOCASE NULL, 
	ITra TEXT COLLATE NOCASE NOT NULL, 
	Apro TEXT COLLATE NOCASE NOT NULL, 
	Medi INTEGER NOT NULL, 
	PINR TEXT COLLATE NOCASE NOT NULL, 
	VRec INTEGER NOT NULL, 
	Abon INTEGER NOT NULL, 
	VSub INTEGER NOT NULL, 
	VKWH INTEGER NOT NULL, 
	Comp INTEGER NOT NULL, 
	MesK INTEGER NOT NULL, 
	MesS INTEGER NOT NULL, 
	CHECK (DATE(FRec) IS NOT NULL), 
	CHECK (TIME(HRec) IS NOT NULL), 
	CHECK (LENGTH(CSeg) <= 100), 
	CHECK (LENGTH(ITra) <= 50), 
	CHECK (LENGTH(Apro) <= 100), 
	CHECK (LENGTH(PINR) <= 100), 
	CHECK (VRec BETWEEN 0 AND 999999999999999), 
	CHECK (Abon BETWEEN 0 AND 10000), 
	CHECK (VSub BETWEEN 0 AND 999999999999999), 
	CHECK (VKWH BETWEEN 0 AND 999999999), 
	CHECK (Comp BETWEEN 0 AND 99999999999), 
	CHECK (MesK BETWEEN 0 AND 99999999999), 
	CHECK (MesS BETWEEN 0 AND 999999999999999), 
	FOREIGN KEY(Empr) REFERENCES TEmpre(ID) ON DELETE RESTRICT, 
	FOREIGN KEY(Medi) REFERENCES TMedid(ID) ON DELETE RESTRICT
);

-- Vistas
--

-- Medidores: Clientes/Propietarios | Valores inferidos
CREATE VIEW IF NOT EXISTS VClien AS 
SELECT
	C.ID AS ID, 
	C.TDoc AS TDoc, 
	C.NDoc AS NDoc, 
	C.Nomb AS Nomb, 
	CASE C.Sexo 
        WHEN 0 THEN 'Femenino' 
        WHEN 1 THEN 'Masculino' 
    END AS Sexo  
FROM TClien C;

-- Medidores | Valores inferidos
CREATE VIEW IF NOT EXISTS VMedid AS 
SELECT 
    Me.ID AS ID, 
	Me.MNIC AS MNIC, 
	Me.Medi AS Medi, 
	Me.Dire AS Dire, 
	CU.CUso AS CUso, 
	Cl.TDoc || ' ' || Cl.NDoc AS Docu, 
	Cl.Nomb AS Nomb 
FROM TMedid Me 
LEFT JOIN TCaUso CU ON Me.CUso = CU.ID 
LEFT JOIN TClien Cl ON Me.Prop = Cl.ID;

-- Recargas | Valores inferidos
CREATE VIEW IF NOT EXISTS VRecar AS 
SELECT 
	Re.ID AS ID, 
	Em.RaSo AS RaSo, 
	Em.NDoc AS NDoc, 
	Re.FRec AS FRec, 
	Re.HRec AS HRec, 
	IIF(Re.CSeg IS NOT NULL AND REPLACE(Re.CSeg, ' ', '') <> '', Re.CSeg, null) AS CSeg, 
	Re.ITra AS ITra, 
	Re.Apro AS Apro, 
	Me.Medi AS Medi, 
	Me.Dire AS Dire, 
	Me.CUso AS CUso, 
	Me.Docu AS Docu, 
	Me.Nomb AS Nomb, 
	Re.PINR AS PINR, 
	Re.VRec AS VRec, 
	Re.Abon AS Abon, 
	Re.VSub AS VSub, 
	Re.VKWH AS VKWH, 
	Re.Comp AS Comp, 
	Re.MesK AS MesK, 
	Re.MesS AS MesS 
FROM TRecar Re 
LEFT JOIN TEmpre Em ON Re.Empr = Em.ID 
LEFT JOIN VMedid Me ON Re.Medi = Me.ID;

-- Triggers
--

-- TEmpre: INSERT
CREATE TRIGGER IF NOT EXISTS TEmpreIN 
AFTER INSERT ON TEmpre 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPos) 
	VALUES (
        'TEmpre', 
		'INSERT', 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('NDoc: ', NEW.NDoc), 
            CONCAT('RaSo: ', NEW.RaSo), 
            CONCAT('Dire: ', NEW.Dire), 
            CONCAT('Tele: ', NEW.Tele), 
            CONCAT('Mail: ', NEW.Mail)
		)
	);
END;

-- TEmpre: UPDATE
CREATE TRIGGER IF NOT EXISTS TEmpreUp 
AFTER UPDATE ON TEmpre 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre, RPos) 
	VALUES (
        'TEmpre', 
		'UPDATE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('NDoc: ', OLD.NDoc), 
            CONCAT('RaSo: ', OLD.RaSo), 
            CONCAT('Dire: ', OLD.Dire), 
            CONCAT('Tele: ', OLD.Tele), 
            CONCAT('Mail: ', OLD.Mail)
		), 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('NDoc: ', NEW.NDoc), 
            CONCAT('RaSo: ', NEW.RaSo), 
            CONCAT('Dire: ', NEW.Dire), 
            CONCAT('Tele: ', NEW.Tele), 
            CONCAT('Mail: ', NEW.Mail)
		)
	);
END;

-- TEmpre: DELETE
CREATE TRIGGER IF NOT EXISTS TEmpreDe 
AFTER DELETE ON TEmpre 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre) 
	VALUES (
        'TEmpre', 
		'DELETE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('NDoc: ', OLD.NDoc), 
            CONCAT('RaSo: ', OLD.RaSo), 
            CONCAT('Dire: ', OLD.Dire), 
            CONCAT('Tele: ', OLD.Tele), 
            CONCAT('Mail: ', OLD.Mail)
		)
	);
END;

-- TCaUso: INSERT
CREATE TRIGGER IF NOT EXISTS TCaUsoIN 
AFTER INSERT ON TCaUso 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPos) 
	VALUES (
        'TCaUso', 
		'INSERT', 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('CUso: ', NEW.CUso), 
            CONCAT('CDes: ', NEW.CDes)
		)
	);
END;

-- TCaUso: UPDATE
CREATE TRIGGER IF NOT EXISTS TCaUsoUp 
AFTER UPDATE ON TCaUso 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre, RPos) 
	VALUES (
        'TCaUso', 
		'UPDATE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('CUso: ', OLD.CUso), 
            CONCAT('CDes: ', OLD.CDes)
		), 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('CUso: ', NEW.CUso), 
            CONCAT('CDes: ', NEW.CDes)
		)
	);
END;

-- TCaUso: DELETE
CREATE TRIGGER IF NOT EXISTS TCaUsoDe 
AFTER DELETE ON TCaUso 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre) 
	VALUES (
        'TCaUso', 
		'DELETE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('CUso: ', OLD.CUso), 
            CONCAT('CDes: ', OLD.CDes)
		)
	);
END;

-- TClien: INSERT
CREATE TRIGGER IF NOT EXISTS TClienIN 
AFTER INSERT ON TClien 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPos) 
	VALUES (
        'TClien', 
		'INSERT', 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('TDoc: ', NEW.TDoc), 
			CONCAT('NDoc: ', NEW.NDoc), 
            CONCAT('Nomb: ', NEW.Nomb), 
			CONCAT('Sexo: ', NEW.Sexo)
		)
	);
END;

-- TClien: UPDATE
CREATE TRIGGER IF NOT EXISTS TClienUp 
AFTER UPDATE ON TClien 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre, RPos) 
	VALUES (
        'TClien', 
		'UPDATE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('TDoc: ', OLD.TDoc), 
			CONCAT('NDoc: ', OLD.NDoc), 
            CONCAT('Nomb: ', OLD.Nomb), 
			CONCAT('Sexo: ', OLD.Sexo)
		), 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('TDoc: ', NEW.TDoc), 
			CONCAT('NDoc: ', NEW.NDoc), 
            CONCAT('Nomb: ', NEW.Nomb), 
			CONCAT('Sexo: ', NEW.Sexo)
		)
	);
END;

-- TClien: DELETE
CREATE TRIGGER IF NOT EXISTS TClienDe 
AFTER DELETE ON TClien 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre) 
	VALUES (
        'TClien', 
		'DELETE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('TDoc: ', OLD.TDoc), 
			CONCAT('NDoc: ', OLD.NDoc), 
            CONCAT('Nomb: ', OLD.Nomb), 
			CONCAT('Sexo: ', OLD.Sexo)
		)
	);
END;

-- TMedid: INSERT
CREATE TRIGGER IF NOT EXISTS TMedidIN 
AFTER INSERT ON TMedid 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPos) 
	VALUES (
        'TMedid', 
		'INSERT', 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('MNIC: ', NEW.MNIC), 
			CONCAT('Medi: ', NEW.Medi), 
			CONCAT('Dire: ', NEW.Dire), 
			CONCAT('CUso: ', NEW.CUso), 
            CONCAT('Prop: ', NEW.Prop)
		)
	);
END;

-- TMedid: UPDATE
CREATE TRIGGER IF NOT EXISTS TMedidUp 
AFTER UPDATE ON TMedid 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre, RPos) 
	VALUES (
        'TMedid', 
		'UPDATE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('MNIC: ', OLD.MNIC), 
			CONCAT('Medi: ', OLD.Medi), 
			CONCAT('Dire: ', OLD.Dire), 
			CONCAT('CUso: ', OLD.CUso), 
            CONCAT('Prop: ', OLD.Prop)
		), 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('MNIC: ', NEW.MNIC), 
			CONCAT('Medi: ', NEW.Medi), 
			CONCAT('Dire: ', NEW.Dire), 
			CONCAT('CUso: ', NEW.CUso), 
            CONCAT('Prop: ', NEW.Prop)
		)
	);
END;

-- TMedid: DELETE
CREATE TRIGGER IF NOT EXISTS TMedidDe 
AFTER DELETE ON TMedid 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre) 
	VALUES (
        'TMedid', 
		'DELETE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('MNIC: ', OLD.MNIC), 
			CONCAT('Medi: ', OLD.Medi), 
			CONCAT('Dire: ', OLD.Dire), 
			CONCAT('CUso: ', OLD.CUso), 
            CONCAT('Prop: ', OLD.Prop)
		)
	);
END;

-- TRecar: INSERT
CREATE TRIGGER IF NOT EXISTS TRecarIN 
AFTER INSERT ON TRecar 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPos) 
	VALUES (
        'TRecar', 
		'INSERT', 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('Empr: ', NEW.Empr), 
			CONCAT('FRec: ', NEW.FRec), 
			CONCAT('HRec: ', NEW.HRec), 
			CONCAT('CSeg: ', NEW.CSeg), 
			CONCAT('ITra: ', NEW.ITra), 
			CONCAT('Apro: ', NEW.Apro), 
			CONCAT('Medi: ', NEW.Medi), 
			CONCAT('PINR: ', NEW.PINR), 
			CONCAT('VRec: ', NEW.VRec), 
			CONCAT('Abon: ', NEW.Abon), 
			CONCAT('VSub: ', NEW.VSub), 
			CONCAT('VKWH: ', NEW.VKWH), 
			CONCAT('Comp: ', NEW.Comp), 
			CONCAT('MesK: ', NEW.MesK), 
			CONCAT('MesS: ', NEW.MesS)
		)
	);
END;

-- TRecar: UPDATE
CREATE TRIGGER IF NOT EXISTS TRecarUp 
AFTER UPDATE ON TRecar 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre, RPos) 
	VALUES (
        'TRecar', 
		'UPDATE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('Empr: ', OLD.Empr), 
			CONCAT('FRec: ', OLD.FRec), 
			CONCAT('HRec: ', OLD.HRec), 
			CONCAT('CSeg: ', OLD.CSeg), 
			CONCAT('ITra: ', OLD.ITra), 
			CONCAT('Apro: ', OLD.Apro), 
			CONCAT('Medi: ', OLD.Medi), 
			CONCAT('PINR: ', OLD.PINR), 
			CONCAT('VRec: ', OLD.VRec), 
			CONCAT('Abon: ', OLD.Abon), 
			CONCAT('VSub: ', OLD.VSub), 
			CONCAT('VKWH: ', OLD.VKWH), 
			CONCAT('Comp: ', OLD.Comp), 
			CONCAT('MesK: ', OLD.MesK), 
			CONCAT('MesS: ', OLD.MesS)
		), 
		CONCAT_WS(' | ', 
			CONCAT('ID: ', NEW.ID), 
			CONCAT('Empr: ', NEW.Empr), 
			CONCAT('FRec: ', NEW.FRec), 
			CONCAT('HRec: ', NEW.HRec), 
			CONCAT('CSeg: ', NEW.CSeg), 
			CONCAT('ITra: ', NEW.ITra), 
			CONCAT('Apro: ', NEW.Apro), 
			CONCAT('Medi: ', NEW.Medi), 
			CONCAT('PINR: ', NEW.PINR), 
			CONCAT('VRec: ', NEW.VRec), 
			CONCAT('Abon: ', NEW.Abon), 
			CONCAT('VSub: ', NEW.VSub), 
			CONCAT('VKWH: ', NEW.VKWH), 
			CONCAT('Comp: ', NEW.Comp), 
			CONCAT('MesK: ', NEW.MesK), 
			CONCAT('MesS: ', NEW.MesS)
		)
	);
END;

-- TRecar: DELETE
CREATE TRIGGER IF NOT EXISTS TRecarDe 
AFTER DELETE ON TRecar 
FOR EACH ROW 
BEGIN 
	INSERT INTO THistory (Tabl, Oper, RPre) 
	VALUES (
        'TRecar', 
		'DELETE', 
        CONCAT_WS(' | ', 
			CONCAT('ID: ', OLD.ID), 
			CONCAT('Empr: ', OLD.Empr), 
			CONCAT('FRec: ', OLD.FRec), 
			CONCAT('HRec: ', OLD.HRec), 
			CONCAT('CSeg: ', OLD.CSeg), 
			CONCAT('ITra: ', OLD.ITra), 
			CONCAT('Apro: ', OLD.Apro), 
			CONCAT('Medi: ', OLD.Medi), 
			CONCAT('PINR: ', OLD.PINR), 
			CONCAT('VRec: ', OLD.VRec), 
			CONCAT('Abon: ', OLD.Abon), 
			CONCAT('VSub: ', OLD.VSub), 
			CONCAT('VKWH: ', OLD.VKWH), 
			CONCAT('Comp: ', OLD.Comp), 
			CONCAT('MesK: ', OLD.MesK), 
			CONCAT('MesS: ', OLD.MesS)
		)
	);
END;
